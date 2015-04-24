using System.Windows;


namespace LauncherZ.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        public void FocusInput()
        {
            CtlInputBox.Focus();
        }

        public void SelectInputText()
        {
            CtlInputBox.SelectAll();
        }

        public void SelectInputText(int start, int length)
        {
            CtlInputBox.Select(start, length);
        }

        public void ClearInputSelection()
        {
            CtlInputBox.Select(CtlInputBox.Text.Length, 0);
        }

        private void MainWindow_Activated(object sender, System.EventArgs e)
        {
            FocusInput();
        }


    }

}
