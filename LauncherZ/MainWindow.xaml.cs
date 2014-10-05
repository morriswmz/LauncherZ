using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LauncherZ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Stack<string> _commanStack = new Stack<string>(); 

        public MainWindow()
        {
            InitializeComponent();
            CtlTaskInput.FocusText();
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //CtlTaskInput.Text = "1";
        }

        private void CtlTaskInput_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Tab))
            {
                CtlTaskInput.HintText = CtlTaskInput.Text;
                e.Handled = true;
            }
            
        }


    }
}
