using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using LauncherZLib.Task;
using LauncherZLib.Task.Provider;

namespace LauncherZ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Stack<string> _commanStack = new Stack<string>();
        private DetailWindow detailWindow;
        private TaskDataList sampleList = new TaskDataListDesignTime();

        public MainWindow()
        {
            InitializeComponent();
            CtlTaskInput.FocusText();
            //CtlTaskList.DataContext = new TaskDataListDesignTime();
            CtlTaskListBox.ItemsSource = sampleList;
            CtlTaskListBox.SelectedIndex = 0;
            var loader = new TaskProviderLoader();
            loader.LoadAllFrom(@".\Providers");
            var cultureInfo = Thread.CurrentThread.CurrentCulture;

        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //CtlTaskInput.Text = "1";
        }

        private void CtlTaskInput_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            
            
        }


        private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Tab))
            {
                CtlTaskInput.HintText = CtlTaskInput.Text;
                sampleList[0].Description += "\nNewLine";
                if (detailWindow == null)
                {
                    detailWindow = new DetailWindow();
                    detailWindow.Show();
                    detailWindow.Top = Top;
                    detailWindow.Left = Left + ActualWidth + 10;
                }
                detailWindow.CtlDetailText.Text = CtlTaskInput.Text;
                e.Handled = true;
            }
        }

        private void CtlTaskListBox_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            
        }

        private void CtlTaskListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null && listBox.Items.Count > 0 && listBox.SelectedIndex == -1)
            {
                listBox.SelectedIndex = 0;
            }
        }
    }
}
