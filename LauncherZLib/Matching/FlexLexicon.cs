using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.Matching
{

    /// <summary>
    /// Defines a mapping from special characters (including surrogate pairs)
    /// to a simple char. This will make it possible to use English letters to
    /// represent Chinese characters and other non-English characters.
    /// </summary>
    public class FlexLexicon
    {
        private readonly Dictionary<string, string> _dict = new Dictionary<string, string>(2048);

        /// <summary>
        /// <para>Checks if given literal has short form match.</para>
        /// <para>For example, with pinying, chinese character "汽" can match "z" or "g", which is the first
        /// letter of its pinying "zhong" or "gai".</para>
        /// </summary>
        /// <param name="literal"></param>
        /// <param name="abbr"></param>
        /// <returns></returns>
        public bool Match(string literal, char abbr)
        {
            if (literal[0] == abbr)
                return true;
            string abbrs;
            if (!_dict.TryGetValue(literal, out abbrs))
                return false;
            if (abbrs[0] == abbr)
                return true;
            return abbrs.IndexOf(abbr, 1) >= 0;
        }

        /// <summary>
        /// <para>Adds entries from file.</para>
        /// <para>The specified file must be a UTF-8 encoded text file. Each non-empty line shoule either
        /// be a comment line or definition line. A comment line starts with "#". A definition line
        /// should have the format "LITERAL:ABBRS" (e.g. "中:z").</para>
        /// </summary>
        /// <param name="path"></param>
        public void AddFromFile(string path)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears the lexicon.
        /// </summary>
        public void Clear()
        {
            _dict.Clear();
        }

    }
}
