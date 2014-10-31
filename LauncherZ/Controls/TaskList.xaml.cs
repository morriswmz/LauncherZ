using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LauncherZLib.Task;

namespace LauncherZ.Controls
{
    /// <summary>
    /// Interaction logic for TaskList.xaml
    /// </summary>
    public partial class TaskList : UserControl
    {

        #region Dependency Properties
        
        [Category("Layout")]
        [Description("Gets or sets the maximum view port height.")]
        public double MaxViewPortHeight
        {
            get { return (double)GetValue(MaxViewPortHeightProperty); }
            set { SetValue(MaxViewPortHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxViewPortHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxViewPortHeightProperty =
            DependencyProperty.Register("MaxViewPortHeight", typeof(double), typeof(TaskList), new FrameworkPropertyMetadata(200.0, FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region Events

        

        #endregion

        private LinkedList<TaskDataEntry> _activeEntries = new LinkedList<TaskDataEntry>();
        private Queue<TaskDataEntry> _recycledEntries = new Queue<TaskDataEntry>(); 
        private TaskDataList _taskDataList;
        private int _selectedIndex;


        public TaskList()
        {
            
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            CtlTaskPanel.SizeChanged += CtlTaskPanelOnSizeChanged;
        }


        /// <summary>
        /// Sets or gets the index of selected entry.
        /// If given index is out of boundary, it will be clamped and no exception will
        /// be thrown.
        /// </summary>
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = Math.Min(Math.Max(0, value), _taskDataList.Count - 1);
                EnsureSelectedVisible();
            }
        }

        public void SelectNext()
        {
            
        }

        public void SelectPrevious()
        {
            
        }

        /// <summary>
        /// Adjusts the top offset to ensure that the selected item is within view port. 
        /// </summary>
        public void EnsureSelectedVisible()
        {
            
        }

        private TaskDataEntry CreateNewEntry(TaskData taskData)
        {
            TaskDataEntry entry;
            if (_recycledEntries.Count > 0)
            {
                entry = _recycledEntries.Dequeue();
                // todo: clean up
            }
            else
            {
                entry = new TaskDataEntry();
            }
            // todo: initialize entry
            entry.DataContext = taskData;
            return entry;
        }

        private void AddTaskAt(int index, TaskData taskData)
        {
            
        }

        private void RecycleEntry(TaskDataEntry entry)
        {
            var taskData = entry.DataContext as TaskData;
            entry.DataContext = null;
            entry.Recycle();
            _activeEntries.Remove(entry);
            _recycledEntries.Enqueue(entry);
        }


        private void CtlTaskPanelOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // if any entry is selected, adjust the offset to keep its relative position
            // with respect to view port unchaged. 

        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                OnTaskDataChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                if (_taskDataList != null)
                {
                    _taskDataList.CollectionChanged -= OnTaskDataChanged;
                    _taskDataList = null;
                }
            }
            else
            {
                var newList = e.NewValue as TaskDataList;
                // safely ignore non-compatible data context
                if (newList != null)
                {
                    if (_taskDataList != null)
                        _taskDataList.CollectionChanged -= OnTaskDataChanged;
                    _taskDataList = newList;
                    _taskDataList.CollectionChanged += OnTaskDataChanged;
                    // reset entries
                    OnTaskDataChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    OnTaskDataChanged(this, new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add, new List<TaskData>(newList), 0));
                }
            }
        }

        private void OnTaskDataChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (var i = 0;i < e.NewItems.Count;i++)
                    {
                        var taskData = e.NewItems[i] as TaskData;
                        if (taskData != null)
                        {
                            TaskDataEntry entry = CreateNewEntry(taskData);
                            CtlTaskPanel.Children.Insert(e.NewStartingIndex + i, entry);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:

                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    // this is not used
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (e.NewItems == null || e.NewItems.Count == 0)
                    {

                    }
                    else
                    {
                        for (var i = 0; i < e.NewItems.Count; i++)
                        {
                            var taskData = e.NewItems[i] as TaskData;
                            if (taskData != null)
                            {
                                TaskDataEntry entry = CreateNewEntry(taskData);
                                CtlTaskPanel.Children.Insert(e.NewStartingIndex + i, entry);
                            }
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

}
