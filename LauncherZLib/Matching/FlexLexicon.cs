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
        private readonly Dictionary<string, char> _dict = new Dictionary<string, char>();

        /// <summary>
        /// <para>Checks if given literal has short form match.</para>
        /// <para>For example, with pinying, chinese character "中" can match "z", which is the first
        /// letter of its pinying "zhong"</para>
        /// </summary>
        /// <param name="literal"></param>
        /// <param name="abbr"></param>
        /// <returns></returns>
        public bool Match(string literal, char abbr)
        {
            if (literal[0] == abbr)
                return true;
            char c;
            return _dict.TryGetValue(literal, out c) && c == abbr;
        }

        

    }
}
