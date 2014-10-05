using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace LauncherZ.Controls
{
    public class FormattedTextBehavior
    {


        public static readonly DependencyProperty FormattedTextProperty =
            DependencyProperty.RegisterAttached("FormattedText", typeof(string), typeof(FormattedTextBehavior), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, OnFormattedTextChanged));

        public static void SetFormattedText(TextBlock d, string value)
        {
            d.SetValue(FormattedTextProperty, value);
        }

        public static string GetFormattedText(TextBlock d)
        {
            return (string) d.GetValue(FormattedTextProperty);
        }

        private static void OnFormattedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = d as TextBlock;
            var text = e.NewValue as string;
            if (textBlock != null)
            {
                textBlock.Inlines.Clear();
                foreach (var segment in ParseFormattedText(text))
                {
                    switch (segment.Format)
                    {
                        case Format.Normal:
                            textBlock.Inlines.Add(new Run(segment.Text));
                            break;
                        case Format.Bold:
                            textBlock.Inlines.Add(new Bold(new Run(segment.Text)));
                            break;
                        case Format.Italic:
                            textBlock.Inlines.Add(new Italic(new Run(segment.Text)));
                            break;
                        case Format.Underline:
                            textBlock.Inlines.Add(new Underline(new Run(segment.Text)));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

        }

        private static IEnumerable<FormattedSegment> ParseFormattedText(string text)
        {
            var result = new List<FormattedSegment>();
            if (string.IsNullOrEmpty(text))
                return result;
            if (text.Length == 1)
            {
                // only one character, discard single '\' if possible
                if (text[0] != '\\')
                    result.Add(new FormattedSegment(Format.Normal, text));
                return result;
            }

            var idx = 0;
            var sb = new StringBuilder();
            while (idx < text.Length)
            {
                var c = text[idx];
                // check escape
                if (c == '\\')
                {
                    // check end, discard single '\'
                    if (idx == text.Length - 1)
                    {
                        if (sb.Length > 0)
                            result.Add(new FormattedSegment(Format.Normal, sb.ToString()));
                        break;
                    }
                    // unescape, simply return whatever sits after '\'
                    idx++;
                    sb.Append(text[idx]);
                    idx++;
                }
                else if ("[~_".IndexOf(c) >= 0)
                {
                    // check formatted segment
                    FormattedSegment segment;
                    switch (c)
                    {
                        case '[':
                            segment = CreateFormattedSegment(text, ref idx, ']', Format.Bold);
                            break;
                        case '~':
                            segment = CreateFormattedSegment(text, ref idx, '~', Format.Italic);
                            break;
                        case '_':
                            segment = CreateFormattedSegment(text, ref idx, '_', Format.Underline);
                            break;
                        default:
                            // should never reach here
                            segment = new FormattedSegment(Format.Normal, "");
                            break;
                    }
                    if (!string.IsNullOrEmpty(segment.Text))
                    {
                        // save buffered normal text
                        if (sb.Length > 0)
                        {
                            result.Add(new FormattedSegment(Format.Normal, sb.ToString()));
                            sb = new StringBuilder();
                        }
                        // add segment
                        result.Add(segment);
                    }
                }
                else
                {
                    sb.Append(c);
                    idx++;
                }
            }
            // final piece
            if (sb.Length > 0)
            {
                result.Add(new FormattedSegment(Format.Normal, sb.ToString()));
            }
            return result;
        }

        private static FormattedSegment CreateFormattedSegment(string text, ref int startIdx, char rightDelimiter,
            Format format)
        {
            int endIdx = IndexOfUnescapedChar(text, rightDelimiter, startIdx + 1);
            string str;
            if (endIdx > startIdx)
            {
                str = text.Substring(startIdx + 1, endIdx - startIdx - 1);
                startIdx = endIdx + 1;
            }
            else
            {
                // no-right delimiter found, auto-closing
                str = text.Substring(startIdx + 1);
                startIdx = text.Length;
            }
            return new FormattedSegment(format, str);
        }

        private static int IndexOfUnescapedChar(string str, char c, int startIdx)
        {
            if (startIdx >= str.Length || str.Length == 0)
                return -1;
            var idx = startIdx;
            while (idx < str.Length)
            {
                if (str[idx] == c && (idx == 0 || str[idx - 1] != '\\'))
                    return idx;
                idx++;
            }
            return -1;
        }

        
        enum Format
        {
            Normal,
            Bold,
            Italic,
            Underline
        }

        struct FormattedSegment
        {
            public readonly Format Format;
            public readonly string Text;

            public FormattedSegment(Format format, string text)
            {
                Format = format;
                Text = text;
            }
            
        }

    }
}
