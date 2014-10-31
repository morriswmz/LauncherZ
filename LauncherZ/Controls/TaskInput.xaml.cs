using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace LauncherZ.Controls
{
    /// <summary>
    /// Interaction logic for TaskInput.xaml
    /// </summary>
    public partial class TaskInput : UserControl
    {


        [Description("Gets or sets the text above the input text box.")]
        public string HintText
        {
            get { return (string)GetValue(HintTextProperty); }
            set { SetValue(HintTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HintText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HintTextProperty =
            DependencyProperty.Register("HintText", typeof(string), typeof(TaskInput), new PropertyMetadata(""));


        [Description("Gets or sets the content of the input text box.")]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TaskInput), new PropertyMetadata(""));



        public TaskInput()
        {
            InitializeComponent();
            
        }

        public void FocusText()
        {
            CtlTextBox.Focus();
        }

    }
}
