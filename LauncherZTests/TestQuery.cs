using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using LauncherZLib.Launcher;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LauncherZTests
{
    /// <summary>
    /// Summary description for TestQuery
    /// </summary>
    [TestClass]
    public class TestQuery
    {
        public TestQuery()
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
        [Timeout(1000)]
        public void TestArgumentParsing()
        {
            string[] inputs =
            {
                " Engineering  is \t\t awesome! ",
                "Engineering  \"is  awesome\"!",
                "Engineering\"is\"awesome!",
                "Engineering \"is awesome!",
                "Engineering \"\"is awesome\"\"!"
            };
            string[][] correctArgs =
            {
                new[] {"Engineering", "is", "awesome!"},
                new[] {"Engineering", "is  awesome", "!"},
                new[] {"Engineering", "is", "awesome!"},
                new[] {"Engineering", "is awesome!"},
                new[] {"Engineering", "is", "awesome", "!"}
            };
            var ids = new long[inputs.Length];
            for (var i = 0; i < inputs.Length; i++)
            {
                var q = new LauncherQuery(inputs[i]);
                CollectionAssert.AreEqual(correctArgs[i], q.Arguments.ToArray(),
                    string.Format("Argument parsing failed for: {0}. Expected: [\"{1}\"]. Actual: [\"{2}\"].",
                    inputs[i], string.Join("\", \"", correctArgs[i]), string.Join("\", \"", q.Arguments)));
                ids[i] = q.QueryId;
            }
            Assert.IsTrue(ids.Distinct().Count() == inputs.Length, "Query ids are not distinct.");
        }
    }
}
