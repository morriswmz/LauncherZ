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

        private void MainWindow_Activated(object sender, System.EventArgs e)
        {
            FocusInput();
        }


    }

}
