using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using LauncherZLib.Utils;

namespace CorePlugins.UnitConverter
{
    public class ConversionSystem
    {
        private static readonly Regex InvalidCharPattern = new Regex(@"[\s\.]");

        private ILogger _logger;
        // dimension -> conversion table
        private Dictionary<string, ConversionTable> _conversionTables = new Dictionary<string, ConversionTable>();
        // dimension -> possible units
        private Dictionary<string, HashSet<string>> _unitGroups = new Dictionary<string, HashSet<string>>();
        // alias -> possible full unit names (i.e., {dim}.{unit})
        // Note: it is possible that one alias maps to multiple units. However, such situations
        // are not frequent. Therefore a samll list is used instead of a heavy hash set
        private Dictionary<string, List<string>> _aliasMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        // dimension -> list of conversion rules
        private Dictionary<string, List<ConversionDefinition>> _loadedConversions = new Dictionary<string, List<ConversionDefinition>>();

        private HashSet<string> _loadedDefinitionFiles = new HashSet<string>(); 

        public ConversionSystem(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            _logger = logger;
#if DEBUG
            ResumeOnError = false;
#else
            ResumeOnError = true;
#endif
        }

        public bool ResumeOnError { get; set; }

        public void LoadAliasesFromLauguageFile(string path)
        {
            Dictionary<string, string> locDict;
            try
            {
                locDict = JsonUtils.StreamDeserialize<Dictionary<string, string>>(path);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load aliases from language file \"{0}\". Details:{1}{2}",
                    path, Environment.NewLine, ex);
                return;
            }
            foreach (var pair in locDict)
            {
                if (string.IsNullOrWhiteSpace(pair.Value))
                {
                    continue;
                }
                string fullUnitName = pair.Key.EndsWith(".plural")
                    ? pair.Key.Substring(0, pair.Key.LastIndexOf('.'))
                    : pair.Key;
                int dotIdx = fullUnitName.IndexOf('.');
                if (dotIdx < 0)
                {
                    continue;
                }
                string dim = fullUnitName.Substring(0, dotIdx);
                string unit = fullUnitName.Substring(dotIdx + 1);
                if (!CheckUnitName(unit))
                {
                    continue;
                }
                AddAlias(pair.Value, dim, unit);
            }
        }

        public void LoadDefinitionsFrom(string path)
        {
            if (_loadedDefinitionFiles.Contains(path))
            {
                return;
            }
            DefinitionFile df;
            try
            {
                df = JsonUtils.StreamDeserialize<DefinitionFile>(path);
            }
            catch (Exception ex)
            {
                if (ResumeOnError)
                {
                    _logger.Error("Cannot load definitions from \"{0}\". Details:{1}{2}",
                        path, Environment.NewLine, ex);
                    return;
                }
                throw;
            }
            foreach (var cg in df.ConversionGroups)
            {
                if (string.IsNullOrWhiteSpace(cg.Dimension) || InvalidCharPattern.IsMatch(cg.Dimension))
                {
                    var msg = string.Format(
                        "Dimension name cannot be empty nor contain white spaces and dots. Skipped " +
                        "conversion group with dimension name \"{0}\".", cg.Dimension);
                    if (ResumeOnError)
                    {
                        _logger.Warning(msg);
                        continue;
                    }
                    throw new FormatException(msg);
                }
                if (!_unitGroups.ContainsKey(cg.Dimension))
                {
                    _unitGroups.Add(cg.Dimension, new HashSet<string>());
                }
                // load units
                foreach (var ud in cg.Units)
                {
                    if (string.IsNullOrWhiteSpace(ud.Name) ||
                        string.IsNullOrWhiteSpace(ud.Aliases))
                    {
                        if (ResumeOnError)
                        {
                            _logger.Warning("Skipped an incomplete unit definition.");
                            continue;
                        }
                        throw new FormatException("Unit name and aliases cannot be empty.");
                    }
                    if (!CheckUnitName(ud.Name))
                    {
                        var msg = string.Format(
                            "Invalid unit name \"{0}\". A unit name cannot contain any white spaces, " +
                            "and may contain one dot in the middle.", ud.Name);
                        if (ResumeOnError)
                        {
                            _logger.Warning(msg);
                            continue;
                        }
                        throw new FormatException(msg);
                    }
                    // register unit if not present
                    _unitGroups[cg.Dimension].Add(ud.Name);
                    // add/merge aliases
                    var aliases = ud.Aliases.Split('|')
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s));
                    foreach (var alias in aliases)
                    {
                        var s = alias == "#" ? ud.Name : alias;
                        AddAlias(s, cg.Dimension, ud.Name);
                    }
                }
                // append new conversions
                if (!_loadedConversions.ContainsKey(cg.Dimension))
                {
                    _loadedConversions[cg.Dimension] = new List<ConversionDefinition>(cg.Conversions);
                }
                else
                {
                    _loadedConversions[cg.Dimension].AddRange(cg.Conversions);
                }
            }
            // all units and conversion definitions loaded, now rebuild those tables, haha
            // but, remember to clear old ones first!
            _conversionTables.Clear();
            foreach (var pair in _unitGroups.Where(p => _loadedConversions.ContainsKey(p.Key)))
            {
                string dimension = pair.Key;
                var convTable = new ConversionTable(dimension, pair.Value);
                foreach (var cd in _loadedConversions[dimension])
                {
                    if (string.IsNullOrEmpty(cd.From) ||
                        string.IsNullOrEmpty(cd.To))
                    {
                        if (ResumeOnError)
                        {
                            _logger.Warning("Skipped an incomplete conversion definition.");
                            continue;
                        }
                        throw new FormatException("Empty unit name in conversion definition.");
                    }
                    // check conversion factor
                    if (double.IsNaN(cd.Factor) || double.IsInfinity(cd.Factor) || Math.Abs(cd.Factor) < double.Epsilon)
                    {
                        var msg = string.Format(
                            "Conversion factor must be non-zero and finite. Violation: \"{0}\" -> \"{1}\" [{2}].",
                            cd.From, cd.To, cd.Factor);
                        if (ResumeOnError)
                        {
                            _logger.Warning(msg);
                            continue;
                        }
                        throw new Exception(msg);
                    }
                    try
                    {
                        convTable.SetFactor(cd.From, cd.To, cd.Factor);
                    }
                    catch (Exception ex)
                    {
                        if (ResumeOnError)
                        {
                            _logger.Warning("An exception occurred while loading conversion factor. Details:{0}{1}",
                                Environment.NewLine, ex);
                            continue;
                        }
                        throw;
                    }
                }
                // check table for potential problems
                if (!convTable.IsComplete)
                {
                    _logger.Warning("Conversion table for dimension \"{0}\" is not complete.", dimension);
                }
                if (!convTable.IsConsistent)
                {
                    _logger.Warning("Conversion table for dimension \"{0}\" is not consistent.", dimension);
                }
                _conversionTables[dimension] = convTable;
            }
            // mark the file as loaded
            _loadedDefinitionFiles.Add(path);
        }

        public IEnumerable<ConversionDescription> GetAllConversions(string fromAlias, string toAlias)
        {
            List<string> fullFromUnits;
            List<string> fullToUnits;
            if (!(_aliasMap.TryGetValue(fromAlias, out fullFromUnits) && _aliasMap.TryGetValue(toAlias, out fullToUnits)))
            {
                return Enumerable.Empty<ConversionDescription>();
            }
            string[] dimensions = fullFromUnits.Select(s => s.Substring(0, s.IndexOf('.')))
                .Distinct()
                .Intersect(fullToUnits.Select(s => s.Substring(0, s.IndexOf('.'))).Distinct())
                .ToArray();
            if (dimensions.Length == 0)
            {
                return Enumerable.Empty<ConversionDescription>();
            }
            var results = new List<ConversionDescription>();
            for (var i = 0; i < dimensions.Length; i++)
            {
                string dim = dimensions[i];
                if (!_conversionTables.ContainsKey(dim))
                {
                    continue;
                }
                string[] curFullFromUnits = fullFromUnits.Where(s => s.StartsWith(dim)).ToArray();
                string[] curFullToUnits = fullToUnits.Where(s => s.StartsWith(dim)).ToArray();
                foreach (var pair in ComputePossiblePairs(curFullFromUnits, curFullToUnits))
                {
                    string fromUnit = pair.From.Substring(pair.From.IndexOf('.') + 1);
                    string toUnit = pair.To.Substring(pair.To.IndexOf('.') + 1);
                    double factor;
                    if (_conversionTables[dim].TryGetFactor(fromUnit, toUnit, out factor))
                    {
                        results.Add(new ConversionDescription(pair.From, pair.To, factor));
                    }
                }
            }
            return results;
        }

        private IEnumerable<UnitPair> ComputePossiblePairs(string[] fullFromUnits, string[] fullToUnits)
        {
            // simple cases first
            // 1 -> 1
            if (fullFromUnits.Length == 1 && fullToUnits.Length == 1)
            {
                return new[]
                {
                    new UnitPair
                    {
                        From = fullFromUnits[0],
                        To = fullToUnits[0]
                    }
                };
            }
            // 1 -> multi
            if (fullFromUnits.Length == 1)
            {
                return fullToUnits.Select(u => new UnitPair
                {
                    From = fullFromUnits[0],
                    To = u
                });
            }
            // multi -> 1
            if (fullToUnits.Length == 1)
            {
                return fullFromUnits.Select(u => new UnitPair
                {
                    From = u,
                    To = fullToUnits[0]
                });
            }
            var prefixExtractor = new Func<string, string>(s =>
            {
                int i1 = s.IndexOf('.');
                int i2 = s.IndexOf('.', i1 + 1);
                return i2 >= 0 ? s.Substring(i1 + 1, i2 - i1 - 1) : "";
            });
            // group units by prefixes (us, uk, etc.)
            // under our restriction to unit names, each group should only have exactly one item
            var groupMap = new Dictionary<string, UnitPair>();
            foreach (var g in fullFromUnits.GroupBy(prefixExtractor))
            {
                groupMap[g.Key] = new UnitPair
                {
                    From = g.ToArray()[0]
                };
            }
            foreach (var g in fullToUnits.GroupBy(prefixExtractor).Where(g => groupMap.ContainsKey(g.Key)))
            {
                groupMap[g.Key] = new UnitPair
                {
                    From = groupMap[g.Key].From,
                    To = g.ToArray()[0]
                };
            }
            var pairs = new List<UnitPair>();
            foreach (var pair in groupMap)
            {
                UnitPair unitPair = pair.Value;
                if (unitPair.From != null && unitPair.To != null)
                {
                    // should be like this
                    pairs.Add(unitPair);
                }
            }
            return pairs;
        }

        private bool CheckUnitName(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            if (str[0] == '.' || str[str.Length - 1] == '.')
            {
                return false;
            }
            int ctr = 0;
            foreach (char c in str)
            {
                if (char.IsWhiteSpace(c))
                {
                    return false;
                }
                if (c == '.')
                {
                    ctr++;
                    if (ctr > 1)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void AddAlias(string alias, string dim, string unit)
        {
            var fullName = dim + '.' + unit;
            if (!_aliasMap.ContainsKey(alias))
            {
                // init a small list
                _aliasMap[alias] = new List<string>(2);
                _aliasMap[alias].Add(fullName);
            }
            // ignore duplicates
            // Note: One alias usually may not refer to two different units under the same dimension
            // unless under different standards. For instance, US gallon (us.gallon) and UK gallon
            // (uk.gallon). The different standard name is added as a prefix here.
            if (_aliasMap[alias].All(s => s != fullName))
            {
                _aliasMap[alias].Add(fullName);
            }
        }

        struct UnitPair
        {
            public string From;
            public string To;
        }

        struct UnitGroupPair
        {
            public string[] FromGroup;
            public string[] ToGroup;
        }
    }

    public class ConversionDescription
    {
        public string FullFromUnitName { get; private set; }
        public string FullToUnitName { get; private set; }
        public double Factor { get; private set; }

        public ConversionDescription(string fullFromUnitName, string fullToUnitName, double factor)
        {
            FullFromUnitName = fullFromUnitName;
            FullToUnitName = fullToUnitName;
            Factor = factor;
        }
    }

}
