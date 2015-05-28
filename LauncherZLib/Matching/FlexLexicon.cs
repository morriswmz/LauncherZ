using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace LauncherZLib.Matching
{

    /// <summary>
    /// Defines a mapping from special characters (including surrogate pairs)
    /// to a simple char. This will make it possible to use English letters to
    /// represent Chinese characters and other non-English characters.
    /// </summary>
    public class FlexLexicon
    {
        /// <summary>
        /// Global lexicon. Share the knowledge here.
        /// </summary>
        public static readonly FlexLexicon GlobalLexicon = new FlexLexicon();

        protected readonly Dictionary<string, string> Dict = new Dictionary<string, string>(2048);

        /// <summary>
        /// <para>Checks if given character has short form match.</para>
        /// <para>For example, with pinying, chinese character "汽" can match "Z" or "G", which is the first
        /// letter of its pinying "zhong" or "gai".</para>
        /// </summary>
        /// <param name="character">Must be one character (possible surrogate pair).</param>
        /// <param name="replacement">Possible replacement character. Will be converted to uppercase
        /// under <b>InvariantCulture</b>.</param>
        /// <returns></returns>
        public virtual bool Match(string character, char replacement)
        {
            string abbrs;
            replacement = char.ToUpperInvariant(replacement);
            if (!Dict.TryGetValue(character, out abbrs))
                return false;
            if (abbrs[0] == replacement)
                return true;
            return abbrs.IndexOf(replacement, 1) >= 0;
        }

        /// <summary>
        /// <para>Adds entries from file.</para>
        /// <para>The specified file must be a UTF-8 encoded text file. Each non-empty line shoule either
        /// be a comment line or definition line. A comment line starts with "#". A definition line
        /// should have the format "CHARS ABBRS" (e.g. "中 Z"), where white spaces functions as
        /// separators.</para>
        /// </summary>
        /// <param name="path"></param>
        public virtual void AddFromFile(string path)
        {
            int lineNo = 0;
            using (var sw = new StreamReader(path, Encoding.UTF8))
            {
                string line;
                while ((line = sw.ReadLine()) != null)
                {
                    lineNo++;
                    // trim
                    line = line.Trim();
                    // ignore empty and comment lines
                    if (line.StartsWith("#") || line.Length == 0)
                        continue;
                    var idx = 0;
                    while (!char.IsWhiteSpace(line[idx]))
                    {
                        idx++;
                        if (idx >= line.Length)
                            throw new FormatException(string.Format(
                                "Syntax error at line {0}:{1}", lineNo, idx));
                    }
                    string charStr = line.Substring(0, idx);
                    while (char.IsWhiteSpace(line[idx]))
                    {
                        idx++;
                        if (idx >= line.Length)
                            throw new FormatException(string.Format(
                                "Syntax error at line {0}:{1}", lineNo, idx));
                    }
                    string abbrStr = line.Substring(idx);
                    Add(charStr, abbrStr);
                }
            }
        }

        /// <summary>
        /// <para>Adds entries to the lexicon.</para>
        /// <para>
        /// For example, <b>Add("事实","s")</b> will map both "事" and "实" to "S".
        /// <b>Add("茄", "jq")</b> will map "茄" to both "J" and "Q".
        /// </para>
        /// </summary>
        /// <param name="character">A string containing all characters.</param>
        /// <param name="replacement">A string containing all possible replacements.</param>
        /// <remarks>
        /// <para>
        /// Do not add entries that may cause ambiguity. For example, <b>Add("a", "b")</b>
        /// is not a good idea (consider "false = true").
        /// </para>
        /// <para>
        /// Replacement characters are converted to uppercase using <b>InvariantCulture</b>
        /// automatically for internal storage.
        /// </para>
        /// </remarks>
        public virtual void Add(string character, string replacement)
        {
            TextElementEnumerator te = StringInfo.GetTextElementEnumerator(character);
            replacement = replacement.ToUpperInvariant();
            while (te.MoveNext())
            {
                string charStr = te.GetTextElement();
                if (Dict.ContainsKey(charStr))
                {
                    // combine existing
                    foreach (char c in replacement)
                    {
                        if (!Dict[charStr].Contains(c))
                        {
                            Dict[charStr] += c;
                        }
                    }
                }
                else
                {
                    // add new
                    Dict.Add(charStr, replacement);
                }
            }
        }

        /// <summary>
        /// Clears the lexicon.
        /// </summary>
        public virtual void Clear()
        {
            Dict.Clear();
        }

    }
}
