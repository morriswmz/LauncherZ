using System;
using System.Collections.Generic;

namespace CorePlugins.UnitConverter
{
    public class UnitInformationRegistry
    {

        private readonly Dictionary<string, List<Unit>> _aliasDict = new Dictionary<string, List<Unit>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<Unit, string> _abbrDict = new Dictionary<Unit, string>();

        /// <summary>
        /// Registers the abbreviation for the specified unit.
        /// Existing registration will be silently overridden.
        /// </summary>
        /// <param name="unit">Specified unit.</param>
        /// <param name="abbr">Abbreviation to be registered. Leading and trailing spaces will be trimmed.</param>
        /// <returns>True if registration is successful.</returns>
        public bool RegisterAbbreviation(Unit unit, string abbr)
        {
            if (abbr == null || unit == null)
            {
                return false;
            }
            abbr = abbr.Trim();
            if (string.IsNullOrWhiteSpace(abbr))
            {
                return false;
            }

            _abbrDict[unit] = abbr;
            RegisterAlias(unit, abbr); // abbreviation also counts as an valid alias
            return true;
        }

        /// <summary>
        /// Registers an alias for the specified unit.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="alias"></param>
        /// <returns>True if registeration is successful.</returns>
        public bool RegisterAlias(Unit unit, string alias)
        {
            if (unit == null || alias == null)
                return false;
            alias = alias.Trim();
            if (string.IsNullOrWhiteSpace(alias))
                return false;

            RegisterAliasImpl(unit, alias);
            return true;
        }
        
        public bool TryGetAbbreviation(Unit unit, out string abbr)
        {
            return _abbrDict.TryGetValue(unit, out abbr);
        }

        public bool TryGetUnitsFromAlias(string alias, out Unit[] units)
        {
            List<Unit> tempUnits;
            if (_aliasDict.TryGetValue(alias, out tempUnits))
            {
                units = tempUnits.ToArray();
                return true;
            }
            units = null;
            return false;
        }

        public void Clear()
        {
            _abbrDict.Clear();
            _aliasDict.Clear();
        }

        private void RegisterAliasImpl(Unit unit, string alias)
        {
            if (_aliasDict.ContainsKey(alias))
            {
                // check existing one before addition
                // Note:
                // Since two units with same dimensions and names, but different standards are treated 
                // as different. One alias may refer to the same type of units under different standards
                // (see detailed information below)
                // One alias usually may not refer to two different units under the same dimension
                // unless under different standards. For instance, US gallon (us.gallon) and UK gallon
                // (uk.gallon). The different standard name is added as a prefix here.
                if (!_aliasDict[alias].Contains(unit))
                {
                    _aliasDict[alias].Add(unit);
                }
            }
            else
            {
                // explicitly initialize a small list since the possibility
                // of sharing same aliases is low
                _aliasDict[alias] = new List<Unit>(2);
                _aliasDict[alias].Add(unit);
            }
        }

    }
}
