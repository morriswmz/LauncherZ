using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LauncherZ.Controls
{
    /// <summary>
    /// Interaction logic for LauncherInput.xaml
    /// </summary>
    public partial class LauncherInput : UserControl
    {
        

        [Description("Gets or sets the text above the input text box.")]
        public string HintText
        {
            get { return (string)GetValue(HintTextProperty); }
            set { SetValue(HintTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HintText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HintTextProperty =
            DependencyProperty.Register("HintText", typeof(string), typeof(LauncherInput), new PropertyMetadata(""));


        [Description("Gets or sets the content of the input text box.")]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(LauncherInput), new PropertyMetadata(""));
        
        [Description("Gets or sets the response delay for text change in milliseconds.")]
        public int ResponseDelay
        {
            get { return (int)GetValue(ResponseDelayProperty); }
            set { SetValue(ResponseDelayProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ResponseDelay.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ResponseDelayProperty =
            DependencyProperty.Register("ResponseDelay", typeof(int), typeof(LauncherInput), new PropertyMetadata(0), ResponseDelayValidateCallback);

        private static bool ResponseDelayValidateCallback(object value)
        {
            return (int) value >= 0;
        }


        public static readonly RoutedEvent DelayedTextChangedEvent = EventManager.RegisterRoutedEvent(
            "DelayedTextChanged", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (LauncherInput));

        [Description("Trigger shortly after text changed.")]
        public event RoutedEventHandler DelayedTextChanged
        {
            add { AddHandler(DelayedTextChangedEvent, value); }
            remove { RemoveHandler(DelayedTextChangedEvent, value); }
        }

        private readonly DispatcherTimer _delayTimer = new DispatcherTimer();
        
        public LauncherInput()
        {
            InitializeComponent();
            _delayTimer.Tick += DelayTimer_Tick;
        }

        

        public bool IsTextFocused
        {
            get { return CtlTextBox.IsFocused; }
        }

        public void FocusText()
        {
            CtlTextBox.Focus();
        }
        
        private void DelayTimer_Tick(object sender, EventArgs eventArgs)
        {
            _delayTimer.Stop();
            RaiseEvent(new RoutedEventArgs(DelayedTextChangedEvent));
        }
        
        private void CtlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _delayTimer.Stop();
            _delayTimer.Interval = new TimeSpan(ResponseDelay * 10000);
            _delayTimer.Start();
        }

    }

}
