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
            var lengthUnits = new[] {"meter", "decimeter", "centimeter", "millimeter", "micrometer", "inch", "foot", "yard"};
            var actualConversions = new[]
            {
                new UnitConversionFactor("millimeter", "centimeter", 0.1),
                new UnitConversionFactor("millimeter", "decimeter", 0.01),
                new UnitConversionFactor("millimeter", "meter", 0.001),
                new UnitConversionFactor("inch", "foot", 1.0/12.0),
                new UnitConversionFactor("inch", "yard", 1.0/(12.0*3.0)),
                new UnitConversionFactor("centimeter", "inch", 1.0/2.54),
                new UnitConversionFactor("meter", "inch", 100.0/2.54),
                new UnitConversionFactor("meter", "foot", 100.0/(2.54*12.0)),
                new UnitConversionFactor("meter", "yard", 100.0/(2.54*12.0*3.0)),
            };
            var ct = new ConversionTable("length", lengthUnits);
            ct.SetFactor("meter", "decimeter", 10.0);
            ct.SetFactor("decimeter","centimeter", 10.0);
            ct.SetFactor("centimeter", "millimeter", 10.0);
            ct.SetFactor("millimeter", "micrometer", 100.0);
            ct.SetFactor("inch", "centimeter", 2.54);
            ct.SetFactor("foot", "inch", 12.0);
            ct.SetFactor("yard", "foot", 3.0);
            double result = 0.0;
            foreach (UnitConversionFactor c in actualConversions)
            {
                Assert.IsTrue(ct.TryConvert(c.From, c.To, 1.0, out result));
                Assert.AreEqual(c.Factor, result, 1e-7);
            }
        }

        class UnitConversionFactor
        {
            public UnitConversionFactor(string @from, string to, double factor)
            {
                From = @from;
                To = to;
                Factor = factor;
            }

            public string From { get; private set; }
            public string To { get; private set; }
            public double Factor { get; private set; }
        }
    }
}
