using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

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
    
}
