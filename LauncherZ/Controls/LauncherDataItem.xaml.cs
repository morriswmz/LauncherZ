using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LauncherZ.Controls
{
    /// <summary>
    /// Interaction logic for LauncherDataItem.xaml
    /// </summary>
    public partial class LauncherDataItem : UserControl
    {

        [Description("Determines whether this entry is highlighted.")]
        public bool Highlighted
        {
            get { return (bool)GetValue(HighlightedProperty); }
            set { SetValue(HighlightedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Highlighted.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightedProperty =
            DependencyProperty.Register("Highlighted", typeof(bool), typeof(LauncherDataItem), 
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        

        public LauncherDataItem()
        {
            InitializeComponent();
            
        }

    }

    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && targetType == typeof(Visibility))
            {
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility && targetType == typeof(bool))
            {
                return (Visibility)value == Visibility.Visible;
            }
            else
            {
                return null;
            }
        }
    }

}
