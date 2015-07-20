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
                CollectionAssert.AreEqual(correctArgs[i], q.InputArguments.ToArray(),
                    string.Format("Argument parsing failed for: {0}. Expected: [\"{1}\"]. Actual: [\"{2}\"].",
                    inputs[i], string.Join("\", \"", correctArgs[i]), string.Join("\", \"", q.InputArguments)));
                ids[i] = q.QueryId;
            }
            Assert.IsTrue(ids.Distinct().Count() == inputs.Length, "Query ids are not distinct.");
        }

        [TestMethod]
        public void TestQueryCreation()
        {
            var uriBasic = "launcherz://user:pass@query:8080/advanced/?input=abc&arg1=10&" +
                           "&arg2=" + Uri.EscapeDataString("中文") +
                           "#?fragment&" + Uri.EscapeDataString("中文");
            var uriDuplicate = "launcherz://query?input=" + Uri.EscapeDataString("红") +
                               "&input=" + Uri.EscapeDataString("橙") +
                               "&input=" + Uri.EscapeDataString("黄");
            var uriNoInput = "launcherz://query";
            var uriIncorrect1 = "launcherz ://query";
            var uriIncorrect2 = "launcherz://query?=&b=c";

            var pcBasic = new QueryParameterCollection(new Dictionary<string, IEnumerable<string>>()
            {
                {"input", new[] {"abc"}},
                {"arg1", new[] {"10"}},
                {"arg2", new[] {"中文"}}
            });
            var newInputBasic = "cba";
            var pcBasicChanged = new QueryParameterCollection(new Dictionary<string, IEnumerable<string>>()
            {
                {"input", new[] {newInputBasic}},
                {"arg1", new[] {"10"}},
                {"arg2", new[] {"中文"}}
            });
            
            var pcDuplicate = new QueryParameterCollection(new Dictionary<string, IEnumerable<string>>()
            {
                {"input", new[] {"红", "橙", "黄"}}
            });
            var newInputDuplicate = "0";
            var pcDuplicateChanged = new QueryParameterCollection(new Dictionary<string, IEnumerable<string>>()
            {
                {"input", new[] {newInputDuplicate}}
            });
            
            var pcNoInput = new QueryParameterCollection();
            var newInputNoInput = "中文";
            var pcNoInputChanged = new QueryParameterCollection(new Dictionary<string, IEnumerable<string>>()
            {
                {"input", new[] {newInputNoInput}}
            });

            // test basic parsing
            var qBasic = new LauncherQuery(new Uri(uriBasic));
            var qDuplicate = new LauncherQuery(new Uri(uriDuplicate));
            var qNoInput = new LauncherQuery(new Uri(uriNoInput));
            Assert.IsTrue(CompareParameterCollection(qBasic.Parameters, pcBasic));
            Assert.IsTrue(CompareParameterCollection(qDuplicate.Parameters, pcDuplicate));
            Assert.IsTrue(CompareParameterCollection(qNoInput.Parameters, pcNoInput));

            // test error handling
            try
            {
                var qIncorrect1 = new LauncherQuery(new Uri(uriIncorrect1));
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is UriFormatException || ex is FormatException);
            }

            try
            {
                var qIncorrect2 = new LauncherQuery(new Uri(uriIncorrect2));
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is UriFormatException || ex is FormatException || ex is ArgumentException);
            }

            // test extending existing query
            var qBasicChanged = new LauncherQuery(qBasic, newInputBasic);
            Assert.AreEqual("launcherz", qBasicChanged.FullUri.Scheme);
            Assert.AreEqual("user:pass", qBasicChanged.FullUri.UserInfo);
            Assert.AreEqual("query:8080", qBasicChanged.FullUri.Authority);
            Assert.AreEqual("/advanced/", qBasicChanged.FullUri.AbsolutePath);
            Assert.AreEqual("#?fragment&中文", Uri.UnescapeDataString(qBasicChanged.FullUri.Fragment));
            Assert.IsTrue(CompareParameterCollection(qBasicChanged.Parameters, pcBasicChanged));

            var qDuplicateChanged = new LauncherQuery(qDuplicate, newInputDuplicate);
            Assert.AreEqual("launcherz", qDuplicateChanged.FullUri.Scheme);
            Assert.AreEqual("query", qDuplicateChanged.FullUri.Authority);
            Assert.IsTrue(CompareParameterCollection(qDuplicateChanged.Parameters, pcDuplicateChanged));

            var qNoInputChanged = new LauncherQuery(qNoInput, newInputNoInput);
            Assert.IsTrue(CompareParameterCollection(qNoInputChanged.Parameters, pcNoInputChanged));

            // test creation by input
            var qUserInput = new LauncherQuery("hello", "internals");
            Assert.AreEqual("internals://query?input=hello", qUserInput.FullUri.ToString());
        }

        protected bool CompareParameterCollection(QueryParameterCollection c1, QueryParameterCollection c2)
        {
            if (c1 == null && c2 == null)
            {
                return true;
            }
            if (c1 == null || c2 == null)
            {
                return false;
            }
            if (ReferenceEquals(c1, c2))
            {
                return true;
            }
            if (c1.Count != c2.Count)
            {
                return false;
            }
            foreach (var pair in c1)
            {
                if (!c2.ContainsKey(pair.Key))
                {
                    return false;
                }
                StringValueCollection v1 = pair.Value;
                StringValueCollection v2 = c2[pair.Key];
                if (v1.Count != v2.Count)
                {
                    return false;
                }
                var counter = new Dictionary<string, int>();
                foreach (var v in v1)
                {
                    if (counter.ContainsKey(v))
                    {
                        counter[v]++;
                    }
                    else
                    {
                        counter[v] = 1;
                    }
                }
                foreach (var v in v2)
                {
                    if (counter.ContainsKey(v))
                    {
                        counter[v]--;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (counter.Any(x => x.Value != 0))
                {
                    return false;
                }
            }
            return true;
        }

    }
}
