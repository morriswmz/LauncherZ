using System;
using System.Collections.Generic;
using System.Linq;

namespace CorePlugins.UnitConverter
{
    public class ConversionSystem
    {
        private readonly UnitInformationRegistry _unitInfoReg;
        // all conversion providers
        private readonly Dictionary<string, IConversionProvider> _conversionProviders =
            new Dictionary<string, IConversionProvider>();

        private bool _loaded = false;

        public ConversionSystem(UnitInformationRegistry unitInfoReg, IEnumerable<IConversionProvider> conversionProviders)
        {
            if (unitInfoReg == null)
                throw new ArgumentNullException("unitInfoReg");
            if (conversionProviders == null)
                throw new ArgumentNullException("conversionProviders");

            _unitInfoReg = unitInfoReg;
            foreach (var conversionProvider in conversionProviders)
            {
                _conversionProviders.Add(conversionProvider.Dimension, conversionProvider);
            }
        }

        public IEnumerable<Conversion> GetAllConversions(string fromAlias, string toAlias)
        {
            Unit[] fromUnits;
            Unit[] toUnits;
            if (!(_unitInfoReg.TryGetUnitsFromAlias(fromAlias, out fromUnits) && _unitInfoReg.TryGetUnitsFromAlias(toAlias, out toUnits)))
            {
                return Enumerable.Empty<Conversion>();
            }
            // compute possible dimensions
            string[] dimensions = fromUnits.Select(u => u.Dimension)
                .Distinct()
                .Intersect(toUnits.Select(u => u.Dimension).Distinct())
                .ToArray();
            if (dimensions.Length == 0)
            {
                return Enumerable.Empty<Conversion>();
            }
            // get all conversions in each dimension
            var results = new List<Conversion>();
            for (var i = 0; i < dimensions.Length; i++)
            {
                string dim = dimensions[i];
                if (!_conversionProviders.ContainsKey(dim))
                {
                    continue;
                }
                Unit[] curFromUnits = fromUnits.Where(s => s.Dimension == dim).ToArray();
                Unit[] curToUnits = toUnits.Where(s => s.Dimension == dim).ToArray();
                foreach (var pair in ComputePossiblePairsInSameDimension(curFromUnits, curToUnits))
                {
                    Func<double, double> converter;
                    if (_conversionProviders[dim].TryProvideConverter(pair.From, pair.To, out converter))
                    {
                        results.Add(new Conversion(pair.From, pair.To, converter));
                    }
                }
            }
            return results;
        }

        private IEnumerable<UnitPair> ComputePossiblePairsInSameDimension(Unit[] fromUnits, Unit[] toUnits)
        {
            // simple cases first
            // 1 -> 1
            if (fromUnits.Length == 1 && toUnits.Length == 1)
            {
                return new[] {new UnitPair(fromUnits[0], toUnits[0])};
            }
            // 1 -> multi
            if (fromUnits.Length == 1)
            {
                return toUnits.Select(u => new UnitPair(fromUnits[0],u));
            }
            // multi -> 1
            if (toUnits.Length == 1)
            {
                return fromUnits.Select(u => new UnitPair(u, toUnits[0]));
            }
            // multi -> multi
            // all non-identity combinations
            var pairs = new List<UnitPair>();
            for (var i = 0; i < fromUnits.Length; i++)
            {
                for (var j = 0; j < toUnits.Length; j++)
                {
                    if (!fromUnits[i].Equals(toUnits[j]))
                    {
                        pairs.Add(new UnitPair(fromUnits[i], toUnits[j]));
                    }
                }
            }
            
            return pairs;
        }

        sealed class UnitPair
        {
            public Unit From { get; private set; }
            public Unit To { get; private set; }

            public UnitPair(Unit fromUnit, Unit toUnit)
            {
                From = fromUnit;
                To = toUnit;
            }
        }
        
        
    }

    public class Conversion
    {
        public Unit From { get; private set; }
        public Unit To { get; private set; }
        public Func<double, double> Converter { get; private set; }

        public Conversion(Unit fromUnit, Unit toUnit, Func<double, double> converter)
        {
            From = fromUnit;
            To = toUnit;
            Converter = converter;
        }
    }

}
