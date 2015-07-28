using System;

namespace CorePlugins.UnitConverter
{
    public interface IConversionProvider
    {
        /// <summary>
        /// Gets the supported dimension.
        /// </summary>
        string Dimension { get; }

        /// <summary>
        /// Attempts to get the converter for specified pair of units.
        /// </summary>
        /// <param name="fromUnit"></param>
        /// <param name="toUnit"></param>
        /// <param name="converter"></param>
        /// <returns>True if converter is provided.</returns>
        bool TryProvideConverter(Unit fromUnit, Unit toUnit, out Func<double, double> converter);

    }
}
