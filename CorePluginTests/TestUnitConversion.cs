using System;
using CorePlugins.UnitConverter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CorePluginTests
{
    [TestClass]
    public class TestUnitConversion
    {

        [TestMethod]
        public void TestConversionTable()
        {
            
            var unitMeter = Unit.FromFullName("length.meter");
            var unitDecimeter = Unit.FromFullName("length.decimeter");
            var unitCentimeter = Unit.FromFullName("length.centimeter");
            var unitMillimeter = Unit.FromFullName("length.millimeter");
            var unitMicrometer = Unit.FromFullName("length.micrometer");
            var unitInch = Unit.FromFullName("length.inch");
            var unitFoot = Unit.FromFullName("length.foot");
            var unitYard = Unit.FromFullName("length.yard");
            var lengthUnits = new[] { unitMeter, unitDecimeter, unitCentimeter, unitMillimeter, unitMicrometer, unitInch, unitFoot, unitYard };

            var actualConversions = new[]
            {
                new UnitConversionFactor(unitMillimeter, unitCentimeter, 0.1),
                new UnitConversionFactor(unitMillimeter, unitDecimeter, 0.01),
                new UnitConversionFactor(unitMillimeter, unitMeter, 0.001),
                new UnitConversionFactor(unitMicrometer, unitMillimeter, 0.001),
                new UnitConversionFactor(unitInch, unitFoot, 1.0/12.0),
                new UnitConversionFactor(unitInch, unitYard, 1.0/(12.0*3.0)),
                new UnitConversionFactor(unitCentimeter, unitInch, 1.0/2.54),
                new UnitConversionFactor(unitMeter, unitInch, 100.0/2.54),
                new UnitConversionFactor(unitMeter, unitFoot, 100.0/(2.54*12.0)),
                new UnitConversionFactor(unitMeter, unitYard, 100.0/(2.54*12.0*3.0)),
            };

            var ct = new AffineConversionTable("length", lengthUnits);
            ct.SetConversionCoefficients(unitMeter, unitDecimeter, 10.0, 0.0);
            ct.SetConversionCoefficients(unitDecimeter, unitCentimeter, 10.0, 0.0);
            ct.SetConversionCoefficients(unitCentimeter, unitMillimeter, 10.0, 0.0);
            ct.SetConversionCoefficients(unitMillimeter, unitMicrometer, 1000.0, 0.0);
            ct.SetConversionCoefficients(unitInch, unitCentimeter, 2.54, 0.0);
            ct.SetConversionCoefficients(unitFoot, unitInch, 12.0, 0.0);
            ct.SetConversionCoefficients(unitYard, unitFoot, 3.0, 0.0);

            foreach (UnitConversionFactor c in actualConversions)
            {
                Func<double, double> converter;
                Assert.IsTrue(ct.TryProvideConverter(c.From, c.To, out converter));
                Assert.AreEqual(c.Factor, converter(1.0), 1e-7);
            }
        }

        class UnitConversionFactor
        {
            public UnitConversionFactor(Unit fromUnit, Unit toUnit, double factor)
            {
                From = fromUnit;
                To = toUnit;
                Factor = factor;
            }

            public Unit From { get; private set; }
            public Unit To { get; private set; }
            public double Factor { get; private set; }
        }
    }
}
