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

        private string _manuallySetText;
        private readonly DispatcherTimer _delayTimer = new DispatcherTimer();
        
        public LauncherInput()
        {
            InitializeComponent();
            _delayTimer.Tick += DelayTimer_Tick;
        }

        #region Properties

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
            return (int)value >= 0;
        }


        public static readonly RoutedEvent DelayedTextChangedEvent = EventManager.RegisterRoutedEvent(
            "DelayedTextChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(LauncherInput));

        [Description("Trigger shortly after text changed.")]
        public event RoutedEventHandler DelayedTextChanged
        {
            add { AddHandler(DelayedTextChangedEvent, value); }
            remove { RemoveHandler(DelayedTextChangedEvent, value); }
        }

        #endregion

        /// <summary>
        /// Gets if the input TextBox is focused.
        /// </summary>
        public bool IsTextFocused
        {
            get { return CtlTextBox.IsFocused; }
        }

        /// <summary>
        /// Attempts to set focus on the input TextBox.
        /// </summary>
        public void FocusText()
        {
            CtlTextBox.Focus();
        }

        /// <summary>
        /// Sets the text without firing DelayedTextChangedEvent.
        /// </summary>
        /// <param name="text"></param>
        public void SetTextWithoutNotification(string text)
        {
            ManuallySetText(text);
        }

        public void SetTextAndNotify(string text)
        {
            ManuallySetText(text);
            RaiseEvent(new RoutedEventArgs(DelayedTextChangedEvent));
        }

        /// <summary>
        /// Moves caret to the end of the input.
        /// </summary>
        public void MoveCaretToEnd()
        {
            CtlTextBox.SelectionStart = CtlTextBox.Text.Length;
            CtlTextBox.SelectionLength = 0;
        }

        /// <summary>
        /// Selects all the input.
        /// </summary>
        public void SelectAll()
        {
            CtlTextBox.SelectAll();
        }

        /// <summary>
        /// Selects part of the input.
        /// Note: This method does not take surrogate pair into consideration.
        /// </summary>
        /// <param name="start">Starting index.</param>
        /// <param name="length">Selection length. Number of UTF-16 charaters.</param>
        public void Select(int start, int length)
        {
            CtlTextBox.SelectionStart = start;
            CtlTextBox.SelectionLength = length;
        }

        private void ManuallySetText(string text)
        {
            // cancel previous delay
            _delayTimer.Stop();
            // set without notify
            _manuallySetText = text;
            CtlTextBox.Text = text;
            MoveCaretToEnd();
        }
        
        private void DelayTimer_Tick(object sender, EventArgs eventArgs)
        {
            _delayTimer.Stop();
            RaiseEvent(new RoutedEventArgs(DelayedTextChangedEvent));
        }
        
        private void CtlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CtlTextBox.Text == _manuallySetText)
            {
                _manuallySetText = null;
                return;
            }

            _manuallySetText = null;
            _delayTimer.Stop();
            _delayTimer.Interval = new TimeSpan(ResponseDelay * 10000);
            _delayTimer.Start();
        }

    }

}
