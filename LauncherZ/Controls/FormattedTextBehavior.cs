using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using LauncherZLib.FormattedText;

namespace LauncherZ.Controls
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
                    textBlock.Inlines.AddRange(FormattedTextEngine.EmitInlines(segments));
                }
                catch (Exception ex)
                {
                    // fail safe
                    textBlock.Text = text;
                }
            }
        }



    }
}
