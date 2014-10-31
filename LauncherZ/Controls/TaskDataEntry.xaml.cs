using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace LauncherZ.Controls
{
    /// <summary>
    /// Interaction logic for TaskDataEntry.xaml
    /// </summary>
    public partial class TaskDataEntry : UserControl
    {

        [Description("Determines whether this entry is highlighted.")]
        public bool Highlighted
        {
            get { return (bool)GetValue(HighlightedProperty); }
            set { SetValue(HighlightedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Highlighted.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightedProperty =
            DependencyProperty.Register("Highlighted", typeof(bool), typeof(TaskDataEntry), 
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        

        public TaskDataEntry()
        {
            InitializeComponent();
            
        }

        public void Recycle()
        {
            
        }

    }
}
