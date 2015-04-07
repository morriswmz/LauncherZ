using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace LauncherZLib.Launcher
{
    public class LauncherList : ICollection<LauncherData>, INotifyCollectionChanged
    {

        protected readonly IComparer<LauncherData> _comparer; 
        protected readonly List<LauncherData> _launchers = new List<LauncherData>();

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public LauncherList(IComparer<LauncherData> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");
            _comparer = comparer;
        }

        public LauncherData this[int index]
        {
            get
            {
                return _launchers[index];
            }
        }

        public int Count
        {
            get { return _launchers.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<LauncherData> GetEnumerator()
        {
            return _launchers.GetEnumerator();
        }

        public virtual void Add(LauncherData item)
        {
            int idx = AddImpl(item);
            DispatchCollectionChangedEvent(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, idx));
        }

        public virtual void AddRange(IEnumerable<LauncherData> commands)
        {
            if (commands == null) return;
            // we have to add one by one as they will be reordered by relevance
            foreach (var commandData in commands)
            {
                Add(commandData);
            }
        }

        public virtual void Clear()
        {
            // clear
            _launchers.Clear();
            // notify
            DispatchCollectionChangedEvent(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)
            );
        }

        public bool Contains(LauncherData item)
        {
            return _launchers.Contains(item);
        }

        public void CopyTo(LauncherData[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex");
            if (arrayIndex + _launchers.Count > array.Length)
                throw new ArgumentException("Insufficient space in given array.");
            foreach (var taskData in _launchers)
            {
                array[arrayIndex++] = taskData;
            }
        }

        public virtual bool Remove(LauncherData item)
        {
            int idx = _launchers.IndexOf(item);
            if (idx > -1)
            {
                _launchers.RemoveAt(idx);
                DispatchCollectionChangedEvent(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, idx));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Actual implemetation of add.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Index of insertion.</returns>
        protected virtual int AddImpl(LauncherData item)
        {
            if (Count == 0)
            {
                _launchers.Add(item);
                return 0;
            }
            // perform binary search and insert
            int l = 0; // highest priority
            int h = Count - 1; // lowest priority
            while (l < h)
            {
                int m = (l + h) / 2;
                int result = _comparer.Compare(item, _launchers[m]);
                if (result > 0)
                {
                    h = m - 1;
                }
                else if (result < 0)
                {
                    l = m + 1;
                }
                else
                {
                    l = h = m;
                }
            }
            // for equal priority, we apply the first-come first-served rule
            // l will be the index for insertion
            int n = Count;
            while (l < n && _comparer.Compare(item, _launchers[l]) <= 0)
            {
                l++;
            }
            _launchers.Insert(l, item);
            return l;
        }

        protected void DispatchCollectionChangedEvent(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

    }

    public class LauncherListDesignTime : LauncherList
    {
        public LauncherListDesignTime() : base(new DummyLauncherComparer())
        {
            base.Add(new LauncherDataDesignTime(1));
            base.Add(new LauncherDataDesignTime(2));
            base.Add(new LauncherDataDesignTime(3));
        }
    }

    class DummyLauncherComparer : IComparer<LauncherData>
    {
        public int Compare(LauncherData x, LauncherData y)
        {
            return 0;
        }
    }

}
