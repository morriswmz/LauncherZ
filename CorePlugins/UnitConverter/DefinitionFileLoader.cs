using System;
using System.Collections.Generic;
using LauncherZLib.Utils;

namespace CorePlugins.UnitConverter
{
    /// <summary>
    /// A single-use definition file loader.
    /// </summary>
    public class DefinitionFileLoader
    {
        private readonly ILogger _logger;
        private readonly string[] _paths;

        private UnitInformationRegistry _unitInformationRegistry;
        private List<IConversionProvider> _conversionProviders; 
        
        /// <summary>
        /// Initialized a new definition file loader.
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="logger"></param>
        public DefinitionFileLoader(string[] paths, ILogger logger)
        {
            if (paths == null)
                throw new ArgumentNullException("paths");
            if (logger == null)
                throw new ArgumentNullException("logger");

            _paths = paths;
            _logger = logger;
        }

        /// <summary>
        /// Gets if loading is done.
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Gets loaded unit information.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="T:System.InvalidOperationException">
        /// Thrown if called before definition are loaded.
        /// </exception>
        public UnitInformationRegistry GetLoadedUnitInformation()
        {
            if (!IsLoaded)
                throw new InvalidOperationException("Files not loaded yet.");

            return _unitInformationRegistry;
        }

        /// <summary>
        /// Gets loaded conversion providers.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="T:System.InvalidOperationException">
        /// Thrown if called before definition are loaded.
        /// </exception>
        public IEnumerable<IConversionProvider> GetLoadedConversionProviders()
        {
            if (!IsLoaded)
                throw new InvalidOperationException("Files not loaded yet.");
            return _conversionProviders;
        }

        /// <summary>
        /// Load all specified definition files.
        /// </summary>
        public void LoadAll()
        {
            if (IsLoaded)
                throw new InvalidOperationException("Definitions have already been loaded.");

            _unitInformationRegistry = new UnitInformationRegistry();
            _conversionProviders = new List<IConversionProvider>();
            var conversionGroups = new Dictionary<string, ConversionGroup>();
            foreach (var path in _paths)
            {
                DefinitionFile defFile;
                try
                {
                    defFile = JsonUtils.StreamDeserialize<DefinitionFile>(path);
                }
                catch (Exception ex)
                {
                    _logger.Error("Cannot load definitions from \"{0}\". Details:{1}{2}",
                        path, Environment.NewLine, ex);
                    continue;
                }
                // read unit definitions first
                foreach (var unitDef in defFile.Units)
                {
                    if (!Unit.IsValidFullName(unitDef.Name))
                    {
                        var msg = string.Format(
                            "Invalid unit name \"{0}\" in \"{1}\". A unit name cannot contain any white spaces, " +
                            "and should follow the format \"dimension[.standardPrefix].name\".",
                            unitDef.Name, path);
                        _logger.Warning(msg);
                    }
                    Unit curUnit = Unit.FromFullName(unitDef.Name);
                    if (!string.IsNullOrEmpty(unitDef.Abbreviation))
                    {
                        if (!_unitInformationRegistry.RegisterAbbreviation(curUnit, unitDef.Abbreviation))
                        {
                            _logger.Warning("Skipped empty abbreviation definition for \"{0}\" in \"{1}\".",
                                curUnit, path);
                        }
                    }
                    if (!string.IsNullOrEmpty(unitDef.Aliases))
                    {
                        string[] aliases = unitDef.Aliases.Split('|');
                        foreach (var alias in aliases)
                        {
                            string aliasToRegister = alias == "#" ? curUnit.Name : alias;
                            if (!_unitInformationRegistry.RegisterAlias(curUnit, aliasToRegister))
                            {
                                _logger.Warning("Skipped an empty alias for \"{0}\" in \"{1}\".", curUnit, path);
                            }
                        }
                    }
                }
                // load all the conversions
                foreach (var convDef in defFile.Conversions)
                {
                    // format check
                    if (!Unit.IsValidFullName(convDef.From) || !Unit.IsValidFullName(convDef.To))
                    {
                        _logger.Warning("Invalid unit name in conversion rule: \"{0}\" -> " +
                                        "\"{1}\". File: \"{2}\".", convDef.From, convDef.To, path);
                        continue;
                    }
                    if (double.IsNaN(convDef.Factor) || Math.Abs(convDef.Factor) < double.Epsilon)
                    {
                        _logger.Warning("Conversion factor is NaN or 0 in conversion rule:" +
                                        "\"{0}\" -> \"{1}\". File: \"{2}\"", convDef.From, convDef.To, path);
                        continue;
                    }
                    if (double.IsNaN(convDef.Offset))
                    {
                        _logger.Warning("Offset is NaN in conversion rule: \"{0}\" -> \"{1}\". File: \"{2}\".",
                            convDef.From, convDef.To, path);
                    }
                    Unit fromUnit = Unit.FromFullName(convDef.From);
                    Unit toUnit = Unit.FromFullName(convDef.To);
                    // logical check
                    if (fromUnit.Dimension != toUnit.Dimension)
                    {
                        _logger.Warning("Dimension mismatch in conversion rule: \"{0}\" -> \"{1}\". File: \"{2}\".",
                            convDef.From, convDef.To, path);
                        continue;
                    }
                    if (fromUnit.Equals(toUnit))
                    {
                        _logger.Warning("Skipped identity conversion: \"{0}\" -> \"{1}\". File: \"{2}\".",
                            convDef.From, convDef.To, path);
                        continue;
                    }
                    // all check passed, add it
                    ConversionGroup conversionGroup;
                    if (!conversionGroups.TryGetValue(fromUnit.Dimension, out conversionGroup))
                    {
                        conversionGroup = new ConversionGroup(fromUnit.Dimension);
                        conversionGroups[fromUnit.Dimension] = conversionGroup;
                    }
                    conversionGroup.Units.Add(fromUnit);
                    conversionGroup.Units.Add(toUnit);
                    conversionGroup.ConversionRules.Add(new ConversionRule(fromUnit, toUnit, convDef.Factor, convDef.Offset));
                }
            }
            // all files read, preparing conversion tables
            foreach (var pair in conversionGroups)
            {
                var table = new AffineConversionTable(pair.Key, pair.Value.Units);
                foreach (var rule in pair.Value.ConversionRules)
                {
                    table.SetConversionCoefficients(rule.From, rule.To, rule.Factor, rule.Offset);
                }
                _conversionProviders.Add(table);
            }

            IsLoaded = true;
        }

        /// <summary>
        /// Auxiliary class for grouping conversion rules under the same dimension.
        /// </summary>
        sealed class ConversionGroup
        {
            public ConversionGroup(string dimension)
            {
                Dimension = dimension;
                ConversionRules = new List<ConversionRule>();
                Units = new HashSet<Unit>();
            }

            public string Dimension { get; private set; }
            public List<ConversionRule> ConversionRules { get; private set; }
            public HashSet<Unit> Units { get; private set; }
        }

        /// <summary>
        /// Auxiliary class for caching conversion rules.
        /// </summary>
        sealed class ConversionRule
        {
            public ConversionRule(Unit fromUnit, Unit toUnit, double factor, double offset)
            {
                From = fromUnit;
                To = toUnit;
                Factor = factor;
                Offset = offset;
            }

            public Unit From { get; private set; }
            public Unit To { get; private set; }
            public double Factor { get; private set; }
            public double Offset { get; private set; }
        }
    }

}
