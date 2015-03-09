using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace LauncherZLib.Matching
{
    public class FlexMatcher
    {

        private int _maxFlexKeywordLength = 16;

        public FlexMatcher()
        {

        }

        /// <summary>
        /// <para>Gets or sets the maximum allowed length of a keyword in flex-matching mode.</para>
        /// <para>This parameter ensures that if given keyword is too long only exact matching will
        /// be performed.</para>
        /// <para>Default value is 16. Minimum value is 1.</para>
        /// </summary>
        public int MaxFlexKeywordLength
        {
            get { return _maxFlexKeywordLength; }
            set { _maxFlexKeywordLength = Math.Max(1, value); }
        }

        /// <summary>
        /// Matches a string against several keywords.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="keywords"></param>
        /// <param name="lexicon"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// This method uses <b>StringComparison.OrdinalIgnoreCase</b> for case insensitive matching
        /// due to the complexity of culture-aware case conversion. Culture-aware culture case
        /// conversion does not necessarily preserve the length of the string, which will make it
        /// difficult to track the indices and content of a match. In addition, current implementation
        /// of regular expression obtains culture information from current thread. It is impossible
        /// to specify culture information in corresponding methods.
        /// </para>
        /// </remarks>
        /// <exception cref="T:System.ArgumentNullException">
        /// Thrown when any of the arguments is null.
        /// </exception>
        public FlexMatchResult Match(string str, string[] keywords, FlexLexicon lexicon)
        {
            if (str == null)
                throw new ArgumentNullException("str");
            if (keywords == null)
                throw new ArgumentNullException("keywords");
            if (lexicon == null)
                throw new ArgumentNullException("lexicon");

            bool exactOnly = keywords.Length > 1;

            // 0: normalize to uppercase
            string strU = str.ToUpperInvariant();
            string[] keywordsU = keywords.Select(kw => kw.ToUpperInvariant()).ToArray();

            // 1: match exact keywords, first come first serve, no overlap
            var allMatched = true;
            var exactMatches = new List<FlexMatch>();
            int idx = 0;
            for (var i = 0;i < keywordsU.Length;i++)
            {
                int foundIdx;
                string keyword = keywordsU[i];
                if (keyword.Length == 0)
                    continue;

                int l = keyword.Length;
                var matched = false;
                idx = 0;
                while ((foundIdx = strU.IndexOf(keyword, idx, StringComparison.Ordinal)) >= 0)
                {
                    idx = foundIdx + 1;
                    // check possible overlap before adding
                    if (i != 0 && exactMatches.Any(m => m.OverlapsWith(foundIdx, l)))
                        continue;
                    exactMatches.Add(new FlexMatch(foundIdx, l, str.Substring(foundIdx, l)));
                    matched = true;
                }
                // for multiple keywords, all of them must be matched
                if (!matched)
                {
                    allMatched = false;
                    break;
                }  
            }
            FlexMatchCollection exactMatchCollection;
            if (allMatched)
            {
                exactMatches.Sort((x, y) => x.StartIndex.CompareTo(y.StartIndex));
                exactMatchCollection = new FlexMatchCollection(exactMatches.ToArray());
            }
            else
            {
                exactMatchCollection = FlexMatchCollection.Empty;
            }
            if (exactOnly || allMatched)
            {
                return new FlexMatchResult(true, exactMatchCollection, false, FlexMatchCollection.Empty);
            }

            // 2: match every character, order is important
            TextElementEnumerator teK = StringInfo.GetTextElementEnumerator(keywordsU[0]);
            TextElementEnumerator teS = StringInfo.GetTextElementEnumerator(strU);
            var flexMatches = new List<FlexMatch>();
            allMatched = true;
            while (teK.MoveNext())
            {
                string kc = teK.GetTextElement();
                var matched = false;
                while (teS.MoveNext())
                {
                    string sc = teS.GetTextElement();
                    // since both keyword and string are normalized
                    // matched = ordinally equal || (kc is simple char && lexicon match)
                    matched = sc.Equals(kc, StringComparison.Ordinal) ||
                                (kc.Length == 1 && lexicon.Match(sc, kc[0]));
                    if (matched)
                    {
                        flexMatches.Add(new FlexMatch(teS.ElementIndex, sc.Length, str.Substring(teS.ElementIndex, sc.Length)));
                        break;
                    }
                }
                // every character should have a match
                if (!matched)
                {
                    allMatched = false;
                    break;
                }
            }
            var flexMatchCollection = allMatched
                ? new FlexMatchCollection(flexMatches.ToArray())
                : FlexMatchCollection.Empty;
            return new FlexMatchResult(true, exactMatchCollection, true, flexMatchCollection);
        }
        

    }
}
