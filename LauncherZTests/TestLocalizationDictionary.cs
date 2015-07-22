using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Collections.Generic;
using LauncherZLib.I18N;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace LauncherZTests
{
    /// <summary>
    /// Summary description for TestUrlRouter
    /// </summary>
    [TestClass]
    [DeploymentItem("I18NSamples", "I18NSamples")]
    public class TestLocalizationDictionary
    {

        private LocalizationDictionary _locDict = new LocalizationDictionary();
        private Dictionary<string, string> _enDict;
        private Dictionary<string, string> _zhDict;

        public TestLocalizationDictionary()
        {
            using (var sw = new StreamReader(@"I18NSamples\lang.en-US.json"))
            {
                string json = sw.ReadToEnd();
                _enDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            using (var sw = new StreamReader(@"I18NSamples\lang.zh-CN.json"))
            {
                string json = sw.ReadToEnd();
                _zhDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
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
        public void TestCultureNameTrimming()
        {
            var fileNames = new string[]
            {
                @"D:\App\i18n\lang",
                @"D:\App\i18n\lang.en-US",
                @"D:\App\i18n\lang.json",
                @"D:\App\i18n\lang.en-US.json",
                "lang",
                "lang.en-US",
                "lang.json",
                "lang.en-US.json"
            };
            var trimResults = new string[]
            {
                @"D:\App\i18n\lang",
                @"D:\App\i18n\lang",
                @"D:\App\i18n\lang.json",
                @"D:\App\i18n\lang.json",
                "lang",
                "lang",
                "lang.json",
                "lang.json"
            };
            var addResults = new string[]
            {
                @"D:\App\i18n\lang.zh-CN",
                @"D:\App\i18n\lang.zh-CN.en-US",
                @"D:\App\i18n\lang.zh-CN.json",
                @"D:\App\i18n\lang.en-US.zh-CN.json",
                "lang.zh-CN",
                "lang.zh-CN.en-US",
                "lang.zh-CN.json",
                "lang.en-US.zh-CN.json"
            };

            TestContext.WriteLine("Testing trimming...");
            for (var i = 0; i < fileNames.Length; i++)
            {
                Assert.AreEqual(trimResults[i], LocalizationHelper.TrimCultureNameFromPath(fileNames[i]));
            }
            TestContext.WriteLine("Testing adding...");
            var culture = new CultureInfo("zh-CN");
            for (var i = 0; i < fileNames.Length; i++)
            {
                Assert.AreEqual(addResults[i], LocalizationHelper.AddCultureNameToPath(fileNames[i], culture));
            }
        }


        [TestMethod]
        public void TestTranslation()
        {
            // prepare
            _locDict.CurrentCulture = new CultureInfo("en-GB");
            _locDict.LoadLanguageFile(@"I18NSamples\lang.json");
            foreach (var pair in _enDict)
            {
                Assert.AreEqual(pair.Value, _enDict[pair.Key]);
            }
            // change
            _locDict.CurrentCulture = new CultureInfo("zh-CN");
            foreach (var pair in _zhDict)
            {
                Assert.AreEqual(pair.Value, _zhDict[pair.Key]);
            }
        }

    }
}
