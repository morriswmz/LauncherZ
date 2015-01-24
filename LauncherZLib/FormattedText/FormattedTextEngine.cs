using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace LauncherZLib.FormattedText
{
    public static class FormattedTextEngine
    {

        public static IEnumerable<FormattedSegment> ParseFormattedText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return Enumerable.Empty<FormattedSegment>();
            
            // remove new lines from the end of the input
            text = text.TrimEnd(Environment.NewLine.ToCharArray());

            var segments = new List<FormattedSegment>();
            int idx = 0, l = text.Length;
            int skip = 0;
            var currentFormat = TextFormat.Normal;
            var sb = new StringBuilder(64);

            while (idx < l)
            {
                char c = text[idx];
                switch (c)
                {
                    case '\n':
                    case '\r':
                        // carriage return and new line
                        // Note: since new line characters are trimmed at the end of the input,
                        // boundary check is unnecessary.
                        // single newline
                        skip = 1;
                        if (c == '\r' && text[idx + 1] == '\n')
                        {
                            // check following
                            skip = 2;
                        }
                        if (sb.Length > 0)
                        {
                            segments.Add(new FormattedSegment(sb.ToString(), currentFormat));
                            sb.Clear();
                        }
                        segments.Add(new FormattedSegment(string.Empty, TextFormat.NewLine));
                        idx += skip;
                        break;
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

        public static IEnumerable<Inline> EmitInlines(IEnumerable<FormattedSegment> segments)
        {
            if (segments == null)
                throw new ArgumentNullException("segments");

            var inlines = new List<Inline>();
            foreach (var segment in segments)
            {
                if (segment.Format.HasFlag(TextFormat.NewLine))
                {
                    inlines.Add(new LineBreak());
                }
                else
                {
                    var run = new Run(segment.Text);
                    if (segment.Format.HasFlag(TextFormat.Bold))
                        run.FontWeight = FontWeights.Bold;
                    if (segment.Format.HasFlag(TextFormat.Italic))
                        run.FontStyle = FontStyles.Italic;
                    if (segment.Format.HasFlag(TextFormat.Underline))
                        run.TextDecorations.Add(TextDecorations.Underline);
                    inlines.Add(run);
                }
            }
            return inlines;
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
