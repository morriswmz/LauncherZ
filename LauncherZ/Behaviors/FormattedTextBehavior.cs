using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using LauncherZLib.FormattedText;

namespace LauncherZ.Behaviors
{
    public static class FormattedTextBehavior
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
                try
                {
                    IEnumerable<FormattedSegment> segments = FormattedTextEngine.ParseFormattedText(text);
                    textBlock.Inlines.Clear();
                    textBlock.Inlines.AddRange(EmitInlines(segments));
                }
                catch (Exception ex)
                {
                    // fail safe
                    textBlock.Text = text;
                }
            }
        }

        /// <summary>
        /// Generates inlines according to given formatted segments.
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
        private static IEnumerable<Inline> EmitInlines(IEnumerable<FormattedSegment> segments)
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
    }
}
