using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using LauncherZLib.Launcher;

namespace LauncherZ.Controls
{
    /// <summary>
    /// Interaction logic for LauncherListView.xaml
    /// </summary>
    public partial class LauncherListView : UserControl
    {

        #region Constants

        private const int NoSelectionUpdateSource = 0;
        private const int FromSelectedIndex = 1;
        private const int FromSelectedLauncher = 2;

        #endregion

        #region Dependency Property Declarations

        [Description("Gets or sets if smooth scrolling is enabled.")]
        public bool SmoothScrolling
        {
            get { return (bool)GetValue(SmoothScrollingProperty); }
            set { SetValue(SmoothScrollingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SmoothScrolling.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SmoothScrollingProperty =
            DependencyProperty.Register("SmoothScrolling", typeof(bool), typeof(LauncherListView), new PropertyMetadata(true));


        [Description("Gets or sets the selected launcher.")]
        public LauncherData SelectedLauncher
        {
            get { return (LauncherData)GetValue(SelectedLauncherProperty); }
            set { SetValue(SelectedLauncherProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedLauncher.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedLauncherProperty =
            DependencyProperty.Register("SelectedLauncher", typeof(LauncherData), typeof(LauncherListView),
            new FrameworkPropertyMetadata(
                null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                SelectedLauncherChangedCallback, SelectedLauncherCoerceCallback));

        private static object SelectedLauncherCoerceCallback(DependencyObject d, object baseValue)
        {
            var llv = (LauncherListView) d;
            // null is acceptable
            if (baseValue == null)
                return null;
            // check selected index, value is acceptable when
            // 1. value == real selected launcher
            // 2. value is an element in laucher list (selection changed but still valid)
            int selectedIdx = llv.SelectedIndex;
            if ((selectedIdx > -1 && selectedIdx < llv._launcherList.Count &&
                 llv._launcherList[selectedIdx] == baseValue) || llv._launcherList.Contains(baseValue))
            {
                return baseValue;
            }

            return DependencyProperty.UnsetValue;
        }

        private static void SelectedLauncherChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var llv = (LauncherListView) d;
            if (llv._selectionUpdateSource != NoSelectionUpdateSource)
                return;

            llv._selectionUpdateSource = FromSelectedLauncher;
            var data = e.NewValue as LauncherData;
            llv.SelectedIndex = data != null ? llv._launcherList.IndexOf(data) : -1;
            llv._selectionUpdateSource = NoSelectionUpdateSource;
        }

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(LauncherListView),
            new FrameworkPropertyMetadata(
                -1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                SelectedIndexChangedCallback, SelectedIndexCoerceCallback
            ), SelectedIndexValidationCallback);

        private static bool SelectedIndexValidationCallback(object value)
        {
            return ((int) value) > -1;
        }

        private static object SelectedIndexCoerceCallback(DependencyObject d, object baseValue)
        {
            var llv = (LauncherListView) d;
            if ((baseValue is int) && (int) baseValue < llv.ItemCount)
                return llv;

            return DependencyProperty.UnsetValue;
        }

        private static void SelectedIndexChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var llv = (LauncherListView) d;
            if (llv._selectionUpdateSource != NoSelectionUpdateSource)
                return;

            llv._selectionUpdateSource = FromSelectedIndex;

            llv._selectionUpdateSource = NoSelectionUpdateSource;
        }

        #endregion

        #region Events

        public delegate void LauncherSelectionChangedEventHandler(object sender, LauncherSelectionChangedEventArgs e);

        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
            "SelectionChanged", RoutingStrategy.Bubble, typeof (LauncherSelectionChangedEventHandler),
            typeof (LauncherListView));

        /// <summary>
        /// Occurs when selection changes.
        /// </summary>
        public event LauncherSelectionChangedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }
        
        #endregion

        #region Private Fields

        private readonly Queue<LauncherDataItem> _recycledItems = new Queue<LauncherDataItem>(); 
        private LauncherList _launcherList;
        private int _selectedIndexOld = -1;

        private bool _updatingHighlightBox = false;
        private double _targetVerticalOffset = 0.0;
        private bool _scrolling = false;

        private int _selectionUpdateSource = false;

        #endregion

        public LauncherListView()
        {
            
            InitializeComponent();
            DataContextChanged += LauncherListView_DataContextChanged;
            CtlLauncherPanel.SizeChanged += CtlLauncherPanelSizeChanged;
            CtlHighlightBox.Visibility = Visibility.Collapsed;
            CompositionTarget.Rendering += CompositionTarget_Rendering;
           
        }


        /// <summary>
        /// Sets or gets the index of selected item.
        /// If given index is out of boundary, it will be clamped and no exception will
        /// be thrown.
        /// </summary>
        public int SelectedIndexOld
        {
            get { return _selectedIndexOld; }
            set
            {
                int oldValue = _selectedIndexOld;
                if (ItemCount > 0)
                {
                    _selectedIndexOld = Math.Min(Math.Max(0, value), CtlLauncherPanel.Children.Count - 1);
                }
                else
                {
                    _selectedIndexOld = -1;
                }
                if (_selectedIndexOld != oldValue)
                {
                    ScheduleUpdateHighlightBox();
                    // possible index adjustment without change actual data
                    LauncherData oldLauncherData = oldValue >= 0 ? GetAssociatedLauncherDataAt(oldValue) : null;
                    LauncherData newLauncherData = _selectedIndexOld >= 0 ? GetAssociatedLauncherDataAt(_selectedIndexOld) : null;
                    if (oldLauncherData != newLauncherData)
                    {
                        RaiseSelectionChangedEvent(oldLauncherData, newLauncherData);
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the LauncherData associated with current selection.
        /// If SelectedIndex is -1, null will be returned.
        /// </summary>
        public LauncherData SelectedLauncherOld
        {
            get { return GetAssociatedLauncherDataAt(SelectedIndexOld); }
            set
            {
                
            }
        }

        /// <summary>
        /// Gets the number of items.
        /// </summary>
        public int ItemCount
        {
            get { return CtlLauncherPanel.Children.Count; }
        }

        

        /// <summary>
        /// Selects the next item.
        /// </summary>
        public void SelectNext()
        {
            int n = CtlLauncherPanel.Children.Count;
            if (n > 0)
            {
                SelectedIndexOld = SelectedIndexOld >= n - 1 ? 0 : SelectedIndexOld + 1;
            }
        }

        /// <summary>
        /// Selects the previous item.
        /// </summary>
        public void SelectPrevious()
        {
            int n = CtlLauncherPanel.Children.Count;
            if (n > 0)
            {
                SelectedIndexOld = SelectedIndexOld > 0 ? SelectedIndexOld - 1 : n - 1;
            }
        }

        /// <summary>
        /// Creates a new item for adding.
        /// </summary>
        /// <param name="launcherData"></param>
        /// <returns></returns>
        private LauncherDataItem CreateNewItem(LauncherData launcherData)
        {
            LauncherDataItem item = _recycledItems.Count > 0 ? _recycledItems.Dequeue() : new LauncherDataItem();
            item.DataContext = launcherData;
            item.SizeChanged += Item_SizeChanged;
            item.Loaded += Item_Loaded;
            return item;
        }

        /// <summary>
        /// Schedules an update of highlight box.
        /// </summary>
        private void ScheduleUpdateHighlightBox()
        {
            if (_updatingHighlightBox)
                return;
            Dispatcher.InvokeAsync(UpdateHighlightBox, DispatcherPriority.Loaded);
        }

        /// <summary>
        /// Actually updates the highlight box, and ensure its visibility.
        /// </summary>
        private void UpdateHighlightBox()
        {
            if (CtlLauncherPanel.Children.Count == 0)
            {
                CtlHighlightBox.Visibility = Visibility.Collapsed;
                _scrolling = false;
                CtlScrollViewer.ScrollToTop();
                _targetVerticalOffset = 0.0;
            }
            else
            {
                CtlHighlightBox.Visibility = Visibility.Visible;
                // CtlLauncherPanel is private and can only contain elements of LauncherDataItem
                var item = (LauncherDataItem) CtlLauncherPanel.Children[SelectedIndexOld];
                Point itemPosition = item.TranslatePoint(new Point(0, 0), CtlLauncherPanel);
                double iy = itemPosition.Y;
                CtlHighlightBox.Width = item.ActualWidth;
                CtlHighlightBox.Height = item.ActualHeight;
                CtlHighlightBox.Margin = new Thickness(0, iy, 0, 0);
                // adjust vertical scroll to make highlight box always visible
                double vh = CtlScrollViewer.ViewportHeight;
                double vo = CtlScrollViewer.VerticalOffset;
                double sh = CtlLauncherPanel.ActualHeight;
                double ih = item.ActualHeight;
                if (vh >= sh)
                {
                    // viewport is large enough
                    _scrolling = false;
                    CtlScrollViewer.ScrollToTop();
                    _targetVerticalOffset = 0.0;
                }
                else
                {
                    // check if selected item is visible
                    if (ih > vh)
                    {
                        // item too big, align at top
                        _targetVerticalOffset = iy;
                        _scrolling = true;
                    }
                    else if (iy - vo > 0.0)
                    {
                        if (iy - vo + ih > vh)
                        {
                            // bottom is clipped, need to move up a little bit
                            _targetVerticalOffset = iy + ih - vh;
                            _scrolling = true;
                        }
                    }
                    else
                    {
                        // top clipped, need to move down a little bit
                        _targetVerticalOffset = iy;
                        _scrolling = true;
                    }
                }
            }
            // check if smooth scrolling is enabled
            if (_scrolling && !SmoothScrolling)
            {
                _scrolling = false;
                CtlScrollViewer.ScrollToVerticalOffset(_targetVerticalOffset);
            }
            _updatingHighlightBox = false;
        }

        /// <summary>
        /// Inserts an new item at specific index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="launcherData"></param>
        private void InsertItemAt(int index, LauncherData launcherData)
        {
            if (index < 0 || index > ItemCount)
                throw new IndexOutOfRangeException();

            // do insertion
            // when a new item is added to the visual tree, loaded event
            // will be triggered after layout is completed, call UpdateHighlightBox()
            // in the event handler to ensure correct size and position
            LauncherDataItem item = CreateNewItem(launcherData);
            if (ItemCount == 0)
            {
                // first item
                CtlLauncherPanel.Children.Add(item);
                // auto select first item
                SelectedIndexOld = 0;
            }
            else
            {
                if (SelectedIndexOld == 0 && index == 0)
                {
                    // if 1st item is selected, we don't change selection when
                    // inserting at index 0
                    LauncherData oldLauncherData = GetAssociatedLauncherDataAt(0);
                    CtlLauncherPanel.Children.Insert(index, item);
                    RaiseSelectionChangedEvent(oldLauncherData, launcherData);
                }
                else
                {
                    // adjust selection if inserted before selection
                    CtlLauncherPanel.Children.Insert(index, item);
                    if (index <= SelectedIndexOld)
                    {
                        SelectedIndexOld++;
                    }
                }
            }
        }

        /// <summary>
        /// Adds an new items to the end of the list.
        /// </summary>
        /// <param name="launcherData"></param>
        private void AddItem(LauncherData launcherData)
        {
            InsertItemAt(CtlLauncherPanel.Children.Count, launcherData);   
        }

        /// <summary>
        /// Removes an item at specific index and adjusts selection accordingly.
        /// </summary>
        /// <param name="idx"></param>
        private void RemoveItemAt(int idx)
        {
            if (idx < 0 || idx >= ItemCount)
                throw new IndexOutOfRangeException();

            LauncherData oldLauncherData = GetAssociatedLauncherDataAt(_selectedIndexOld);
            
            DoRemoveItem(idx);

            // update scroll and selection
            // we need to use internel field and raise event here since item is removed
            // and we can no longer access it in the setter of SelectedIndex
            if (ItemCount == 0)
            {
                _selectedIndexOld = -1;
                ScheduleUpdateHighlightBox();
                RaiseSelectionChangedEvent(oldLauncherData, null);
                return;
            }
            if (idx < SelectedIndexOld || (idx == SelectedIndexOld && idx == ItemCount))
            {
                _selectedIndexOld--;
                ScheduleUpdateHighlightBox();
                LauncherData newLauncherData = GetAssociatedLauncherDataAt(_selectedIndexOld);
                if (newLauncherData != oldLauncherData)
                {
                    RaiseSelectionChangedEvent(oldLauncherData, newLauncherData);
                }
            }
        }

        /// <summary>
        /// Removes all items and reset.
        /// </summary>
        private void RemoveAll()
        {
            if (CtlLauncherPanel.Children.Count == 0)
                return;

            LauncherData oldLauncherData = GetAssociatedLauncherDataAt(_selectedIndexOld);

            while (CtlLauncherPanel.Children.Count > 0)
            {
                DoRemoveItem(0);
            }
            // update selection, scroll will be automatically adjusted
            _selectedIndexOld = -1;
            ScheduleUpdateHighlightBox();
            RaiseSelectionChangedEvent(oldLauncherData, null);
        }

        /// <summary>
        /// Removes an item and its event handlers, resets its DataContext.
        /// </summary>
        /// <param name="idx"></param>
        private void DoRemoveItem(int idx)
        {
            var item = (LauncherDataItem)CtlLauncherPanel.Children[idx];
            item.DataContext = null;
            item.SizeChanged -= Item_SizeChanged;
            item.Loaded -= Item_Loaded;
            CtlLauncherPanel.Children.RemoveAt(idx);
            _recycledItems.Enqueue(item);
        }

        private LauncherData GetAssociatedLauncherDataAt(int idx)
        {
            if (idx < 0 || idx >= CtlLauncherPanel.Children.Count)
                return null;

            var item = (LauncherDataItem)CtlLauncherPanel.Children[idx];
            return item.DataContext as LauncherData;
        }

        private void RaiseSelectionChangedEvent(LauncherData oldItem, LauncherData newItem)
        {
            var e = new LauncherSelectionChangedEventArgs(
                SelectionChangedEvent, this, oldItem, newItem);
            RaiseEvent(e);
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (_scrolling)
            {
                // smooth scrolling !!!!
                CtlScrollViewer.ScrollToVerticalOffset(0.5 * (_targetVerticalOffset + CtlScrollViewer.VerticalOffset));
                if (Math.Abs(CtlScrollViewer.VerticalOffset - _targetVerticalOffset) < 0.5)
                {
                    CtlScrollViewer.ScrollToVerticalOffset(_targetVerticalOffset);
                    _scrolling = false;
                }
            }
        }

        private void Item_Loaded(object sender, RoutedEventArgs routedEventArgs)
        {
            ScheduleUpdateHighlightBox();
        }

        private void Item_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // one of the item has its size changed.
            if (SelectedIndexOld >= CtlLauncherPanel.Children.IndexOf((LauncherDataItem)sender))
            {
                ScheduleUpdateHighlightBox();
            }
        }

        private void CtlLauncherPanelSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // if any item is selected, adjust the offset to keep its relative position
            // with respect to view port unchaged. 

        }

        private void LauncherListView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                LauncherListCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                if (_launcherList != null)
                {
                    _launcherList.CollectionChanged -= LauncherListCollectionChanged;
                    _launcherList = null;
                }
            }
            else
            {
                var newList = e.NewValue as LauncherList;
                // safely ignore non-compatible data context
                if (newList != null)
                {
                    if (_launcherList != null)
                        _launcherList.CollectionChanged -= LauncherListCollectionChanged;
                    _launcherList = newList;
                    _launcherList.CollectionChanged += LauncherListCollectionChanged;
                    // reset entries
                    LauncherListCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    LauncherListCollectionChanged(this, new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add, new List<LauncherData>(newList), 0));
                }
            }
        }

        private void LauncherListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 0)
                    {
                        // append
                        foreach (object item in e.NewItems)
                        {
                            var launcherData = item as LauncherData;
                            AddItem(launcherData);
                        }
                    }
                    else
                    {
                        // insert
                        for (var i = 0; i < e.NewItems.Count; i++)
                        {
                            var launcherData = e.NewItems[i] as LauncherData;
                            InsertItemAt(e.NewStartingIndex + i, launcherData);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 0)
                    {
                        foreach (var item in e.OldItems)
                        {
                            
                        }
                    }
                    else
                    {
                        
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:

                    break;
                case NotifyCollectionChangedAction.Move:
                    throw new NotSupportedException();
                case NotifyCollectionChangedAction.Reset:
                    // remove all first
                    RemoveAll();
                    // add possible new items
                    if (e.NewItems != null && e.NewItems.Count != 0)
                    {
                        foreach (object t in e.NewItems)
                        {
                            var launcherData = t as LauncherData;
                            AddItem(launcherData);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }

}
