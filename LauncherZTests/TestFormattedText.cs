using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using LauncherZLib.FormattedText;
using LauncherZLib.Matching;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LauncherZTests
{
    /// <summary>
    /// Summary description for TestFormattedText
    /// </summary>
    [TestClass]
    public class TestFormattedText
    {
        public TestFormattedText()
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

        [TestMethod]
        public void TestFormattedTextEngine()
        {
            // plain text
            var inputPlain = "\rLine1\nLine2\r\r\n\r\n";
            var formatPlain = TextFormat.Normal;
            var expectedOutputPlain = new FormattedSegment[]
            {
                new FormattedSegment("Line1", formatPlain),
                new FormattedSegment(string.Empty, TextFormat.NewLine),
                new FormattedSegment("Line2", formatPlain),
                new FormattedSegment(string.Empty, TextFormat.NewLine),
                new FormattedSegment(string.Empty, TextFormat.NewLine)
            };
            FormattedSegment[] actualOutputPlain = FormattedTextEngine.ParsePlainText(inputPlain, formatPlain).ToArray();
            CollectionAssert.AreEqual(expectedOutputPlain, actualOutputPlain);

            // formatted text
            var inputFormatted = "\r[Title] _Underline_\r\n" +
                                 "\\~~[\\[_Mixed_\\]]~\\~\n";
            var expectedOutputFormatted = new FormattedSegment[]
            {
                new FormattedSegment("Title", TextFormat.Bold),
                new FormattedSegment(" ", TextFormat.Normal),
                new FormattedSegment("Underline", TextFormat.Underline),
                new FormattedSegment(string.Empty, TextFormat.NewLine),
                new FormattedSegment("~", TextFormat.Normal),
                new FormattedSegment("[", TextFormat.Italic | TextFormat.Bold), 
                new FormattedSegment("Mixed", TextFormat.Bold | TextFormat.Underline | TextFormat.Italic),
                new FormattedSegment("]", TextFormat.Italic | TextFormat.Bold),
                new FormattedSegment("~", TextFormat.Normal),
                new FormattedSegment(string.Empty, TextFormat.NewLine) 
            };
            FormattedSegment[] actualOutputFormatted = FormattedTextEngine.ParseFormattedText(inputFormatted).ToArray();
            CollectionAssert.AreEqual(expectedOutputFormatted, actualOutputFormatted);

            // match result
            var matcher = new FlexMatcher();
            var inputFlex1 = "A\nBC\nDE\nF";
            // keywords = {"A\nB", "D", "E"};
            var result1 = new FlexMatchResult(
                true, new FlexMatchCollection(new FlexMatch[]
                {
                    new FlexMatch(0, 3, "A\nB"),
                    new FlexMatch(5, 1, "D"),
                    new FlexMatch(6, 1, "E")
                }), false, FlexMatchCollection.Empty);
            var expectedOutputFlex1 = new FormattedSegment[]
            {
                new FormattedSegment("A", TextFormat.Bold),
                new FormattedSegment(string.Empty, TextFormat.NewLine),
                new FormattedSegment("B", TextFormat.Bold),
                new FormattedSegment("C", TextFormat.Normal),
                new FormattedSegment(string.Empty, TextFormat.NewLine),
                new FormattedSegment("DE", TextFormat.Bold),
                new FormattedSegment(string.Empty, TextFormat.NewLine),
                new FormattedSegment("F", TextFormat.Normal)
            };
            FormattedSegment[] actualOutputFlex1 =
                FormattedTextEngine.ParseFlexMatchResult(inputFlex1, result1).ToArray();
            CollectionAssert.AreEqual(expectedOutputFlex1, actualOutputFlex1);

            var inputFlex2 = "Renewable Energy";
            // keywords2 = {"rewag"};
            var result2 = new FlexMatchResult(true, FlexMatchCollection.Empty,
                true, new FlexMatchCollection(new FlexMatch[]
                {
                    new FlexMatch(0, 2, "Re"),
                    new FlexMatch(4, 2, "wa"),
                    new FlexMatch(14, 1, "g")
                }));
            var expectedOutputFlex2 = new FormattedSegment[]
            {
                new FormattedSegment("Re", TextFormat.Bold),
                new FormattedSegment("ne", TextFormat.Normal),
                new FormattedSegment("wa", TextFormat.Bold),
                new FormattedSegment("ble Ener", TextFormat.Normal),
                new FormattedSegment("g", TextFormat.Bold),
                new FormattedSegment("y", TextFormat.Normal)
            };
            FormattedSegment[] actualOutputFlex2 =
                FormattedTextEngine.ParseFlexMatchResult(inputFlex2, result2).ToArray();
            CollectionAssert.AreEqual(expectedOutputFlex2, actualOutputFlex2);

        }
    }
}
