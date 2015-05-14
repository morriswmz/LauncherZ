using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
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

        #region Dependency Property Declarations

        /// <summary>
        /// Gets or sets if smoothing scrolling is enabled.
        /// </summary>
        [Description("Gets or sets if smooth scrolling is enabled.")]
        public bool SmoothScrolling
        {
            get { return (bool)GetValue(SmoothScrollingProperty); }
            set { SetValue(SmoothScrollingProperty, value); }
        }

        public static readonly DependencyProperty SmoothScrollingProperty =
            DependencyProperty.Register("SmoothScrolling", typeof(bool), typeof(LauncherListView), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets the selected launcher.
        /// </summary>
        [Description("Gets or sets the selected launcher.")]
        public LauncherData SelectedLauncher
        {
            get { return (LauncherData)GetValue(SelectedLauncherProperty); }
            set { SetValue(SelectedLauncherProperty, value); }
        }

        public static readonly DependencyProperty SelectedLauncherProperty =
            DependencyProperty.Register("SelectedLauncher", typeof(LauncherData), typeof(LauncherListView),
            new FrameworkPropertyMetadata(
                null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                SelectedLauncherChangedCallback, SelectedLauncherCoerceCallback));

        /// <summary>
        /// Gets the selected index.
        /// </summary>
        [Description("Gets the selected index.")]
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            protected set { SetValue(SelectedIndexPropertyKey, value);}
        }

        public static readonly DependencyPropertyKey SelectedIndexPropertyKey =
            DependencyProperty.RegisterReadOnly("SelectedIndex", typeof(int), typeof(LauncherListView), new PropertyMetadata(-1));

        public static readonly DependencyProperty SelectedIndexProperty = SelectedIndexPropertyKey.DependencyProperty;
        

        #endregion
        
        #region Private Fields

        private readonly Queue<LauncherDataItem> _recycledItems = new Queue<LauncherDataItem>(); 
        private LauncherList _launcherList;

        private bool _updatingHighlightBox = false;
        private double _targetVerticalOffset = 0.0;
        private bool _scrolling = false;

        #endregion

        public LauncherListView()
        {
            
            InitializeComponent();
            DataContextChanged += LauncherListView_DataContextChanged;
            CtlHighlightBox.Visibility = Visibility.Collapsed;
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        #region Static Methods

        private static object SelectedLauncherCoerceCallback(DependencyObject d, object baseValue)
        {
            var llv = (LauncherListView)d;
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
            var llv = (LauncherListView)d;
            var data = e.NewValue as LauncherData;
            // update selection index
            llv.SelectedIndex = data != null ? llv._launcherList.IndexOf(data) : -1;
            llv.ScheduleUpdateHighlightBox();
        }

        #endregion

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
                var item = (LauncherDataItem) CtlLauncherPanel.Children[SelectedIndex];
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
                _targetVerticalOffset = Math.Round(_targetVerticalOffset);
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
            item.Focusable = false;
            return item;
        }


        /// <summary>
        /// Inserts an new launcher at specific index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="launcherData"></param>
        private void InsertItemAt(int index, LauncherData launcherData)
        {
            int itemCount = CtlLauncherPanel.Children.Count;
            if (index < 0 || index > itemCount)
                throw new IndexOutOfRangeException();
            if (launcherData == null)
                throw new ArgumentNullException("launcherData");

            // do insertion
            // when a new item is added to the visual tree, loaded event
            // will be triggered after layout is completed, call UpdateHighlightBox()
            // in the event handler to ensure correct size and position
            LauncherDataItem item = CreateNewItem(launcherData);
            if (itemCount == 0)
            {
                // first item
                CtlLauncherPanel.Children.Add(item);
                // auto select first item
                SelectedLauncher = launcherData;
            }
            else
            {
                CtlLauncherPanel.Children.Insert(index, item);
                int sIdx = SelectedIndex;
                if (sIdx == 0 && index == 0)
                {
                    // if 1st item is selected, we don't change selection index when
                    // inserting at index 0
                    SelectedLauncher = launcherData;
                }
                else if (sIdx >= index)
                {
                    SelectedIndex++;
                    ScheduleUpdateHighlightBox();
                }
            }
            VerifyLaunchers();
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
            int itemCount = CtlLauncherPanel.Children.Count;
            if (idx < 0 || idx >= itemCount)
                throw new IndexOutOfRangeException();

            int originalSelectedIndex = SelectedIndex;
            DoRemoveItem(idx);

            if (itemCount == 0)
            {
                // no launcher left
                SelectedLauncher = null;
                return;
            }
            if (idx < originalSelectedIndex)
            {
                // removed launcher is before the selected one
                // selected launcher is not changed but selection index must be adjusted
                SelectedIndex--;
                ScheduleUpdateHighlightBox();
            }
            else if (idx == originalSelectedIndex)
            {
                // selected launcher is removed, change to next one if possible
                // note the _launcherList has been changed already
                SelectedLauncher = idx == itemCount ? _launcherList[idx - 1] : _launcherList[idx];
            }
            VerifyLaunchers();
        }

        /// <summary>
        /// Removes all launchers and reset.
        /// </summary>
        private void RemoveAll()
        {
            if (CtlLauncherPanel.Children.Count == 0)
                return;

            while (CtlLauncherPanel.Children.Count > 0)
            {
                DoRemoveItem(0);
            }
            // update selection, scroll will be automatically adjusted
            SelectedLauncher = null;
            ScheduleUpdateHighlightBox();
            VerifyLaunchers();
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
            if (SelectedIndex >= CtlLauncherPanel.Children.IndexOf((LauncherDataItem)sender))
            {
                ScheduleUpdateHighlightBox();
            }
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

        [Conditional("DEBUG")]
        private void VerifyLaunchers()
        {
            if (_launcherList != null)
            {
                for (int i = 0; i < CtlLauncherPanel.Children.Count; i++)
                {
                    Debug.Assert(_launcherList[i] == ((LauncherDataItem) CtlLauncherPanel.Children[i]).DataContext,
                        "LauncherData mismatch.");
                }
            }
        }

    }

}
