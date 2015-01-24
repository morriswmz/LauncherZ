using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.Matching;

namespace LauncherZLib.FormattedText
{
    public static class FormattedTextEngine
    {
        /// <summary>
        /// <para>Converts input into formatted segments.</para>
        /// <para>Supports following formats (controlled by special characters):</para>
        /// <list type="bullet">
        ///     <item>
        ///         <term>[text]</term>
        ///         <description>Bold</description>
        ///     </item>
        ///     <item>
        ///         <term>~text~</term>
        ///         <description>Italic</description>
        ///     </item>
        ///     <item>
        ///         <term>_text_</term>
        ///         <description>Underline</description>
        ///     </item>
        /// </list>
        /// <para>
        /// Formats are stackable and escaping is done by backslash.
        /// For example, "\\[" will produce "[" instead of starting a bold text segement.
        /// </para>
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static IEnumerable<FormattedSegment> ParseFormattedText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return Enumerable.Empty<FormattedSegment>();

            var segments = new List<FormattedSegment>();
            int idx = 0, l = text.Length;
            var currentFormat = TextFormat.Normal;
            var sb = new StringBuilder(64);

            while (idx < l)
            {
                char c = text[idx];
                int skip;
                switch (c)
                {
                    case '\n':
                    case '\r':
                        // carriage return and new line
                        // only "\n" and "\r\n" are valid.
                        // a single "\r" will be ignored.
                        skip = 1;
                        bool newline = false;
                        if (c == '\n')
                        {
                            newline = true;
                        }
                        else if (c == '\r' && idx + 1 < l && text[idx + 1] == '\n')
                        {
                            // check following
                            skip = 2;
                            newline = true;
                        }
                        if (newline)
                        {
                            if (sb.Length > 0)
                            {
                                segments.Add(new FormattedSegment(sb.ToString(), currentFormat));
                                sb.Clear();
                            }
                            segments.Add(new FormattedSegment(string.Empty, TextFormat.NewLine));
                        }
                        idx += skip;
                        break;
                    case '\\':
                        // escape
                        if (idx + 1 < l && "[]~_\\".IndexOf(text[idx + 1]) >= 0)
                        {
                            sb.Append(text[idx + 1]);
                            idx += 2;
                        }
                        else
                        {
                            sb.Append('\\');
                            idx++;
                        }
                        break;;
                    case '[':
                    case ']':
                    case '~':
                    case '_':
                        // format controller
                        if (idx + 1 < l && text[idx + 1] == c)
                        {
                            // check escape
                            sb.Append(c);
                            skip = 2;
                        }
                        else
                        {
                            // update format
                            TextFormat newFormat = ModifyFormatByControlChar(currentFormat, c);
                            if (!newFormat.Equals(currentFormat))
                            {
                                if (sb.Length > 0)
                                {
                                    segments.Add(new FormattedSegment(sb.ToString(), currentFormat));
                                    sb.Clear();
                                }
                                currentFormat = newFormat;
                            }
                            skip = 1;
                        }
                        idx += skip;
                        break;
                    default:
                        sb.Append(c);
                        idx++;
                        break;
                }
            }
            // final piece
            if (sb.Length > 0)
            {
                segments.Add(new FormattedSegment(sb.ToString(), currentFormat));
            }
            return segments;
        }

        public static IEnumerable<FormattedSegment> ParseFlexMatchResult(string input, FlexMatchResult result)
        {
            return ParseFlexMatchResult(input, result, TextFormat.Normal, TextFormat.Bold);
        } 

        public static IEnumerable<FormattedSegment> ParseFlexMatchResult(string input, FlexMatchResult result,
            TextFormat normalFormat, TextFormat highlightFormat)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (result == null)
                throw new ArgumentNullException("result");

            if (!result.Success)
                return new FormattedSegment[] {new FormattedSegment(input, TextFormat.Normal)};

            FlexMatchCollection matches = null;
            if (result.IsExactMatchPerformed && result.ExactMatches.Count > 0)
                matches = result.ExactMatches;
            if (result.IsFlexMatchPerformed && result.FlexMatches.Count > 0)
                matches = result.FlexMatches;

            if (matches != null)
            {
                var segments = new List<FormattedSegment>();
                var lastPlainIdx = 0;
                var matchSb = new StringBuilder();
                foreach (var m in matches)
                {
                    if (m.StartIndex > lastPlainIdx)
                    {
                        if (matchSb.Length > 0)
                        {
                            segments.AddRange(ParsePlainText(matchSb.ToString(), highlightFormat));
                            matchSb.Clear();
                        }
                        segments.AddRange(ParsePlainText(input.Substring(lastPlainIdx, m.StartIndex - lastPlainIdx), normalFormat));
                        matchSb.Append(m.Content);
                    }
                    else
                    {
                        // concat adjacent match
                        matchSb.Append(m.Content);
                    }
                    lastPlainIdx = m.StartIndex + m.Length;
                }
                if (matchSb.Length > 0)
                {
                    segments.AddRange(ParsePlainText(matchSb.ToString(), highlightFormat));
                }
                if (lastPlainIdx < input.Length)
                {
                    segments.AddRange(ParsePlainText(input.Substring(lastPlainIdx, input.Length - lastPlainIdx), normalFormat));
                }
                return segments;
            }

            return Enumerable.Empty<FormattedSegment>();
        }

        public static IEnumerable<FormattedSegment> ParsePlainText(string text, TextFormat format)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            var segments = new List<FormattedSegment>();
            int idx = 0, n = text.Length;
            var sb = new StringBuilder();
            while (idx < n)
            {
                char c = text[idx];
                var skip = 1;
                var split = false;
                switch (c)
                {
                    case '\n':
                        split = true;
                        break;
                    case '\r':
                        if (idx + 1 < n && text[idx + 1] == '\n')
                        {
                            skip = 2;
                            split = true;
                        }
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
                if (split)
                {
                    if (sb.Length > 0)
                    {
                        segments.Add(new FormattedSegment(sb.ToString(), format));
                        sb.Clear();
                    }
                    segments.Add(new FormattedSegment(string.Empty, TextFormat.NewLine));
                }
                idx += skip;
            }
            if (sb.Length > 0)
            {
                segments.Add(new FormattedSegment(sb.ToString(), format));
            }
            return segments;
        } 
        
        private static TextFormat ModifyFormatByControlChar(TextFormat format, char c)
        {
            switch (c)
            {
                case '[':
                    return (format | TextFormat.Bold);
                case ']':
                    return (format & (~TextFormat.Bold));
                case '~':
                    return format.HasFlag(TextFormat.Italic)
                        ? (format & (~TextFormat.Italic))
                        : (format | TextFormat.Italic);
                case '_':
                    return format.HasFlag(TextFormat.Underline)
                        ? (format & (~TextFormat.Underline))
                        : (format | TextFormat.Underline);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
