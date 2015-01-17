using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LauncherZTests
{
    /// <summary>
    /// Summary description for BenchmarkMisc
    /// </summary>
    [TestClass]
    public class BenchmarkMisc
    {
        public BenchmarkMisc()
        {
            
        }

        private TestContext _testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return _testContextInstance;
            }
            set
            {
                _testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void BenchmarkLinqArraySelect()
        {
            // generate test data
            var rnd = new Random();
            var testData = new string[2048];
            for (var i = 0; i < testData.Length; i++)
            {
                testData[i] = "abcdedfghijklmnopqrstuvwxyz".Substring(rnd.Next(26), 1);
            }

            var sw = new Stopwatch();
            var repeat = 10000;
            // baseline
            sw.Start();
            var outputBuffer1 = new string[testData.Length];
            for (var t = 0; t < repeat; t++)
            {
                for (var i = 0; i < testData.Length; i++)
                {
                    outputBuffer1[i] = testData[i].ToLower();
                }
            }
            sw.Stop();
            TestContext.WriteLine("Baseline (for): {0}ms", sw.ElapsedMilliseconds);

            // LINQ
            sw.Restart();
            string[] outputBuffer2 = null;
            for (var t = 0; t < repeat; t++)
            {
                outputBuffer2 = testData.Select(s => s.ToLower()).ToArray();
            }
            sw.Stop();

            Assert.AreEqual(outputBuffer1[0], outputBuffer2[0]);
            TestContext.WriteLine("LINQ: {0}ms", sw.ElapsedMilliseconds);
        }
    }
}
