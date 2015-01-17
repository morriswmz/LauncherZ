using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.Matching
{
    public class FlexMatcher
    {

        private readonly FlexLexicon _lexicon = new FlexLexicon();

        private CultureInfo _culture;

        public FlexMatcher(CultureInfo culture)
        {
            if (culture == null)
                throw new ArgumentNullException();
            _culture = culture;
        }

        /// <summary>
        /// Gets or sets the culture environment.
        /// This is used in casing conversion, etc.
        /// </summary>
        public CultureInfo Culture
        {
            get { return _culture; }
            set { _culture = value ?? CultureInfo.InvariantCulture; }
        }

        /// <summary>
        /// Gets the assigned lexicon.
        /// </summary>
        public FlexLexicon Lexicon { get { return _lexicon; } }

        /// <summary>
        /// Matches a string against several keywords.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="keywords"></param>
        /// <returns></returns>
        public FlexResult Match(string str, string[] keywords)
        {
            bool exactOnly = keywords.Length > 1;

            // 0: normalize
            string strCI = str.ToLower(_culture);
            string[] keywordsCI = keywords.Select(kw => kw.ToLower(_culture)).ToArray();


            // 1: match exact keywords, order is irrelevant
            bool allMatched = false;
            var exactMatches = new List<FlexMatch>();
            for (var i = 0;i < keywordsCI.Length;i++)
            {
                int idx = 0;
                int foundIdx;
                string keyword = keywordsCI[i];
                while ((foundIdx = strCI.IndexOf(keyword, idx, StringComparison.Ordinal)) >= 0)
                {
                    exactMatches.Add(new FlexMatch(idx, keyword.Length, keywords[i]));
                    idx = foundIdx + 1;
                }
            }

            if (exactOnly)
            {
                return null;
            }
            // 2: match every character
            TextElementEnumerator te = StringInfo.GetTextElementEnumerator(keywords[0]);
            string s;
            while (te.MoveNext())
            {
                
                s = te.GetTextElement();
            }

            return null;
        }
        

    }
}
