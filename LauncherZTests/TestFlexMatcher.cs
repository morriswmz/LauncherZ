using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using LauncherZLib.Matching;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LauncherZTests
{
    /// <summary>
    /// Summary description for TestFlexMatcher
    /// </summary>
    [TestClass]
    [DeploymentItem("LexiconSamples", "LexiconSamples")]
    public class TestFlexMatcher
    {

        private FlexLexicon _lexicon;

        public TestFlexMatcher()
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
        public void TestFlexLexicon()
        {
            var lexicon = new FlexLexicon();
            lexicon.AddFromFile(@"LexiconSamples\zh-CN.txt");
            IEnumerable<FlexLexiconTestData> testData = LoadFlexLexiconTestData(@"LexiconSamples\zh-CN.testdata.txt");
            foreach (var t in testData)
            {
                Assert.AreEqual(t.ExpectedResult, lexicon.Match(t.Character, t.Replacement));
            }
        }

        [TestMethod]
        public void TestFlexMatching()
        {
            var fm = new FlexMatcher();
            fm.Lexicon.AddFromFile(@"LexiconSamples\zh-CN.txt");

            var englishInput = "Bag Age Gear Bar";
            var englishKeywords1 = new string[] {"ba", "ag", "ge"};
            var englishResult1 = new FlexMatchResult(true,
                new FlexMatchCollection(new FlexMatch[]
                {
                    new FlexMatch(0, 2, "Ba"),
                    new FlexMatch(4, 2, "Ag"),
                    new FlexMatch(8, 2, "Ge"),
                    new FlexMatch(13, 2, "Ba")
                }), false, FlexMatchCollection.Empty);
            var englishKeywords2 = new string[] {"ba", "ak"};
            var englishResult2 = new FlexMatchResult(true, FlexMatchCollection.Empty, false,
                new FlexMatchCollection(new FlexMatch[0]));
            var mixedInput = "中国上海 China Shanghai";
            var mixedKeywords1 = new string[] {"zgshcs"};
            var mixedResult1 = new FlexMatchResult(true, FlexMatchCollection.Empty, true,
                new FlexMatchCollection(new FlexMatch[]
                {
                    new FlexMatch(0, 1, "中"),
                    new FlexMatch(1, 1, "国"),
                    new FlexMatch(2, 1, "上"),
                    new FlexMatch(3, 1, "海"),
                    new FlexMatch(5, 1, "C"),
                    new FlexMatch(11, 1, "S")
                }));
            var mixedKeywords2 = new string[] {"zgsh]"};
            var mixedResult2 = new FlexMatchResult(true, FlexMatchCollection.Empty, true,
               new FlexMatchCollection(new FlexMatch[0]));

            FlexMatchResult result;

            result = fm.Match(englishInput, englishKeywords1);
            CompareResults(englishResult1, result);
            result = fm.Match(englishInput, englishKeywords2);
            CompareResults(englishResult2, result);
            result = fm.Match(mixedInput, mixedKeywords1);
            CompareResults(mixedResult1, result);
            result = fm.Match(mixedInput, mixedKeywords2);
            CompareResults(mixedResult2, result);
        }

        private void CompareResults(FlexMatchResult expected, FlexMatchResult actual)
        {
            Assert.AreEqual(expected.Success, actual.Success, "Success status mismatch.");
            Assert.AreEqual(expected.IsExactMatchPerformed, actual.IsExactMatchPerformed,
                "IsExactMatchPerformed mismatch.");
            Assert.AreEqual(expected.ExactMatches.Count, actual.ExactMatches.Count,
                "Number of exact matches mismatch.");
            for (var i = 0; i < expected.ExactMatches.Count; i++)
            {
                Assert.AreEqual(expected.ExactMatches[i], actual.ExactMatches[i]);
            }
            Assert.AreEqual(expected.IsFlexMatchPerformed, actual.IsFlexMatchPerformed,
                "IsFlexMatchPerformed mismatch.");
            Assert.AreEqual(expected.FlexMatches.Count, actual.FlexMatches.Count,
                "Number of flex matches mismatch.");
            for (var i = 0; i < expected.FlexMatches.Count; i++)
            {
                Assert.AreEqual(expected.FlexMatches[i], actual.FlexMatches[i]);
            }
        }

        private IEnumerable<FlexLexiconTestData> LoadFlexLexiconTestData(string path)
        {
            var data = new List<FlexLexiconTestData>();
            using (var sw = new StreamReader(path))
            {
                string line;
                while ((line = sw.ReadLine()) != null)
                {
                    string[] splits = line.Split(new []{" "}, StringSplitOptions.None);
                    data.Add(new FlexLexiconTestData(splits[0], splits[1][0], bool.Parse(splits[2])));
                }
            }
            return data;
        }


        private class FlexLexiconTestData
        {
            public string Character { get; private set; }
            public char Replacement { get; private set; }
            public bool ExpectedResult { get; private set; }

            public FlexLexiconTestData(string character, char replacement, bool expectedResult)
            {
                Character = character;
                Replacement = replacement;
                ExpectedResult = expectedResult;
            }
        }

    }
}
