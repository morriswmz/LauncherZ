using System;
using System.Text.RegularExpressions;
using LauncherZLib.Plugin.Modules;
using LauncherZLib.Plugin.Modules.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LauncherZTests
{
    /// <summary>
    /// Tests url router
    /// </summary>
    [TestClass]
    public class TestUrlRouter
    {
        public TestUrlRouter()
        {
            
        }

        private TestContext _testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
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

        [TestMethod]
        [Description("Tests url segmentation functions in UrlHelper.")]
        public void TestUrlSegmentation()
        {
            CollectionAssert.AreEqual(
                new[] {"/"},
                UrlHelper.GetSegmentsUnescaped(""));
            CollectionAssert.AreEqual(
                new[] {"/", "abc"},
                UrlHelper.GetSegmentsUnescaped("abc"));
            CollectionAssert.AreEqual(
                new[] {"/", "abc/", "1 2/", "3"},
                UrlHelper.GetSegmentsUnescaped("/abc/1%202/3"));
            CollectionAssert.AreEqual(
                new[] {"/", "abc/", "1 2/", "3"},
                UrlHelper.GetSegmentsUnescaped("/abc/1%202/3?input=none#top"));
            CollectionAssert.AreEqual(
                new[] {"/", "abc/", "1 2/", "3"},
                UrlHelper.GetSegmentsUnescaped("/abc/1%202/3?input=none#original=http://www.google.com"));
            CollectionAssert.AreEqual(
                new[] {"/", "abc/", "1 2/", "3"},
                UrlHelper.GetSegmentsUnescaped("/abc/1%202/3?input=none#top?abc=efg"));
            CollectionAssert.AreEqual(
                new[] {"/", "abc/", "1 2/", "3"},
                UrlHelper.GetSegmentsUnescaped("/abc/1%202/3#/?abc=efg"));

            CollectionAssert.AreEqual(
                new[] {"/"},
                UrlHelper.GetSegmentsUnescaped("http://foo.bar"));
            CollectionAssert.AreEqual(
                new[] {"/", "abc"},
                UrlHelper.GetSegmentsUnescaped("http://foo.bar/abc"));
            CollectionAssert.AreEqual(
                new[] {"/", "abc/", "1 2/", "3"},
                UrlHelper.GetSegmentsUnescaped("http://foo.bar/abc/1%202/3"));
            CollectionAssert.AreEqual(
                new[] {"/", "abc/", "1 2/", "3"},
                UrlHelper.GetSegmentsUnescaped("http://foo.bar/abc/1%202/3?input=none#top"));
            CollectionAssert.AreEqual(
                new[] {"/", "abc/", "1 2/", "3"},
                UrlHelper.GetSegmentsUnescaped("http://foo.bar/abc/1%202/3?input=none#original=http://www.google.com"));
            CollectionAssert.AreEqual(
                new[] {"/", "abc/", "1 2/", "3"},
                UrlHelper.GetSegmentsUnescaped("http://foo.bar/abc/1%202/3?input=none#top?abc=efg"));
            CollectionAssert.AreEqual(
                new[] {"/", "abc/", "1 2/", "3"},
                UrlHelper.GetSegmentsUnescaped("http://foo.bar/abc/1%202/3#/?abc=efg"));

            try
            {
                UrlHelper.GetSegmentsUnescaped("http:///foo.bar");
                Assert.Fail("Exception expected");
            }
            catch
            {
            }
        }

        [TestMethod]
        public void TestBasicRouting()
        {
            var testData = new string[,]
            {
                {"launcherz://domain/", "/", "root"},
                {"launcherz://domain/aaa", "/aaa", "aaa"},
                {"launcherz://domain/aaa/b1", "/aaa/b1", "b1"},
                {"launcherz://domain/aaa/b2", "/aaa/b2", "b2"},
                {"launcherz://domain/aaa/bbb/ccc/ddd/eee", "/aaa/bbb/ccc/ddd/eee", "eee1"},
                {"launcherz://domain/aaa/bbb/ccc/ddd/eee/", "/aaa/bbb/ccc/ddd/eee/", "eee2"},
                {"launcherz://domain/ppp/%25qqq", "/ppp/%25qqq", "escape"}
            };
            var badUrls = new Uri[]
            {
                new Uri("launcherz://domain/zzz"),
                new Uri("launcherz://domain/ppp/%25qqq/")
            };
            var router = new UrlRouter<string>();
            // test correct urls
            for (var i = 0; i < testData.GetLength(0); i++)
            {
                router.Add(testData[i, 1], testData[i, 2]);
            }
            for (var i = 0; i < testData.GetLength(0); i++)
            {
                var result = router.Route(new Uri(testData[i, 0]));
                Assert.IsTrue(result.Success);
                Assert.AreEqual(testData[i,2], result.Handler);
            }
            // test non-exist urls
            for (var i = 0; i < badUrls.Length; i++)
            {
                Assert.IsFalse(router.Route(badUrls[i]).Success);
            }

        }

        [TestMethod]
        public void TestAdvancedRouting()
        {
            var router = new UrlRouter<string>();
            router.Add("/", "root");
            router.Add("/static/css/global.css", "cssfile");
            router.Add("/post/:postid/", "post");
            router.Add("/post/special/", "special");
            try
            {
                router.Add("/post/:postid2/", "post2"); // override above 2 should throw exception
                Assert.Fail("Exception should be thrown.");
            } catch {}
            router.Add("/post/special/", "special2"); // override special case
            router.Add("/user/:userid/:action", "action");
            router.Add(new Regex(@"^/tag/(?<id>\d+)$"), "tag");

            var staticResult = router.Route(new Uri("launcherz://domain/static/css/global.css"));
            Assert.IsTrue(staticResult.Success);
            Assert.AreEqual("cssfile", staticResult.Handler);

            var postResult = router.Route(new Uri("launcherz://domain/post/what-is-new/"));
            Assert.IsTrue(postResult.Success);
            Assert.AreEqual("post", postResult.Handler);
            Assert.AreEqual("what-is-new", postResult.Parameters["postid"]);
            var postBadResult = router.Route(new Uri("launcherz://domain/post/what-is-new"));
            Assert.IsFalse(postBadResult.Success);

            var postSpecialResult = router.Route(new Uri("launcherz://domain/post/special/"));
            Assert.IsTrue(postSpecialResult.Success);
            Assert.AreEqual("special2", postSpecialResult.Handler);

            var userResult = router.Route(new Uri("launcherz://domain/user/15682/%23delete"));
            Assert.IsTrue(userResult.Success);
            Assert.AreEqual("action", userResult.Handler);
            Assert.AreEqual("15682", userResult.Parameters["userid"]);
            Assert.AreEqual("#delete", userResult.Parameters["action"]);
            var userBadResult = router.Route(new Uri("launcherz://domain/user/15682/%23delete/"));
            Assert.IsFalse(userBadResult.Success);

            var tagResult = router.Route(new Uri("launcherz://domain/tag/12520"));
            Assert.IsTrue(tagResult.Success);
            Assert.AreEqual("tag", tagResult.Handler);
            Assert.AreEqual("/tag/12520", tagResult.Parameters["0"]);
            Assert.AreEqual("/tag/12520", tagResult.Parameters[0]);
            Assert.AreEqual("12520", tagResult.Parameters["id"]);
            Assert.AreEqual("12520", tagResult.Parameters[1]);
            var tagBadResult1 = router.Route(new Uri("launcherz://domain/tag/12520/"));
            Assert.IsFalse(tagBadResult1.Success);
            var tagBadResult2 = router.Route(new Uri("launcherz://domain/tag/abcde"));
            Assert.IsFalse(tagBadResult2.Success);
        }

        class AdvancedRoutingTestEntry
        {
            public Uri FullUrl { get; set; }
            public string ExpectedData { get; set; }
            public ParameterCollection ExpectedParams { get; set; }
        }

    }
}
