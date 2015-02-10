using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        /// <summary>
        /// Converts input into plain segments (normal / newline only) with unified format.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="format"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Converts <see cref="T:LauncherZLib.Matching.FlexMatchResult"/> to formatted string,
        /// assuming bold as highlight format.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static string ConvertFlexMatchResult(string input, FlexMatchResult result)
        {
            return ConvertFlexMatchResult(input, result, TextFormat.Normal, TextFormat.Bold);
        } 

        /// <summary>
        /// Converts <see cref="T:LauncherZLib.Matching.FlexMatchResult"/> to formatted string with
        /// specified format.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="result"></param>
        /// <param name="normalFormat"></param>
        /// <param name="highlightFormat"></param>
        /// <returns></returns>
        public static string ConvertFlexMatchResult(string input, FlexMatchResult result,
            TextFormat normalFormat, TextFormat highlightFormat)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            if (result == null)
                throw new ArgumentNullException("result");

            if (!result.Success)
                return WrapWithFormat(input, normalFormat);

            FlexMatchCollection matches = null;
            if (result.IsExactMatchPerformed && result.ExactMatches.Count > 0)
                matches = result.ExactMatches;
            if (result.IsFlexMatchPerformed && result.FlexMatches.Count > 0)
                matches = result.FlexMatches;
            
            if (matches != null)
            {
                var sb = new StringBuilder(input.Length + matches.Count*4);
                var plainStart = 0;
                var hlStart = 0;
                foreach (var m in matches)
                {
                    if (m.StartIndex > plainStart)
                    {
                        if (plainStart > hlStart)
                        {
                            // emit existing match
                            sb.Append(WrapWithFormat(input.Substring(hlStart, plainStart - hlStart), highlightFormat));
                        }
                        sb.Append(WrapWithFormat(input.Substring(plainStart, m.StartIndex - plainStart), normalFormat));
                        hlStart = m.StartIndex;
                        plainStart = m.StartIndex + m.Length;
                    }
                    else
                    {
                        // concat adjacent matches
                        plainStart += m.Length;
                    }
                }
                // last pieces
                if (plainStart > hlStart)
                    sb.Append(WrapWithFormat(input.Substring(hlStart, plainStart - hlStart), highlightFormat));
                if (plainStart < input.Length)
                    sb.Append(WrapWithFormat(input.Substring(plainStart), normalFormat));
                return sb.ToString();
            }


            return WrapWithFormat(input, normalFormat);
        }
        
        /// <summary>
        /// <para>Wraps a string with format controllers.</para>
        /// <para>For example, WrapWithFormat("A", TextFormat.Bold) will return "[A]".</para>
        /// </summary>
        /// <param name="str"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method will fix potential escaping problems. If the given string ends with a single backslash,
        /// a new backslash will be appended to prevent escaping controlling characters.
        /// </remarks>
        public static string WrapWithFormat(string str, TextFormat format)
        {
            if (string.IsNullOrEmpty(str))
                throw new ArgumentNullException("str");

            bool fixEscape = str.Length >= 2 && str[str.Length - 1] == '\\' & str[str.Length - 2] != '\\';
            if (format == TextFormat.Normal || format.HasFlag(TextFormat.Underline))
                return fixEscape ? str + '\\' : str;
            
            var left = format.HasFlag(TextFormat.Bold) ? "[" : "";
            var right = format.HasFlag(TextFormat.Bold) ? "]" : "";
            if (format.HasFlag(TextFormat.Italic))
            {
                left = left + '~';
                right = '~' + right;
            }
            if (format.HasFlag(TextFormat.Underline))
            {
                left = left + '_';
                right = '_' + right;
            }
            return fixEscape
                ? string.Format("{0}{1}\\{2}", left, str, right)
                : string.Format("{0}{1}{2}", left, str, right);
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
