using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Controls;
using LauncherZ.Task;

namespace LauncherZ.Controls
{
    /// <summary>
    /// Interaction logic for TaskList.xaml
    /// </summary>
    public partial class TaskList : UserControl
    {

        private ObservableCollection<TaskData> taskDatas = new ObservableCollection<TaskData>();
        

        public TaskList()
        {
            InitializeComponent();
            taskDatas.CollectionChanged += OnTaskDataChanged;
        }

        private void OnTaskDataChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
