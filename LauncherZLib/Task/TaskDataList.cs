using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace LauncherZLib.Task
{
    public class TaskDataList : ICollection<TaskData>, INotifyCollectionChanged
    {

        private readonly Dictionary<string, double> _priorityMap = new Dictionary<string, double>(); 
        private readonly List<TaskData> _tasks = new List<TaskData>();
        private readonly Dictionary<string, SubListNode> _subListNodeMap = new Dictionary<string, SubListNode>();
        private readonly SubListNode _subListNodeHead = new SubListNode(0, 0, "");  

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public TaskData this[int index]
        {
            get
            {
                return _tasks[index];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<TaskData> GetEnumerator()
        {
            return _tasks.GetEnumerator();
        }

        public void Add(TaskData item)
        {
            if (Contains(item)) return;
            // retrieve sublist node 
            string providerId = item.MetaData.ProviderId;
            SubListNode node = _subListNodeMap.ContainsKey(providerId) ?
                _subListNodeMap[providerId] : InitNewSubList(providerId);
            // insert
            int insertIdx = node.Start + node.Count;
            _tasks.Insert(insertIdx, item);
            // update following sublists
            OffsetFollowingSubListNodes(node, 1);
            // notify
            DispatchCollectionChangedEvent(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, insertIdx)
            );
        }

        public void Clear()
        {
            // clear
            _tasks.Clear();
            _subListNodeMap.Clear();
            // notify
            DispatchCollectionChangedEvent(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)
            );
        }

        public bool Contains(TaskData item)
        {
            return _tasks.Contains(item);
        }

        public void CopyTo(TaskData[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex");
            if (arrayIndex + _tasks.Count > array.Length)
                throw new ArgumentException("Insufficient space in given array.");
            foreach (var taskData in _tasks)
            {
                array[arrayIndex++] = taskData;
            }
        }

        public bool Remove(TaskData item)
        {
            if (!Contains(item)) return false;
            // remove
            int idx = _tasks.IndexOf(item);
            _tasks.Remove(item);
            // update following sublists
            SubListNode node = _subListNodeMap[item.MetaData.ProviderId];
            node.Count--;
            OffsetFollowingSubListNodes(node, -1);
            // notify
            DispatchCollectionChangedEvent(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, idx)
            );
            return true;
        }

        public int Count
        {
            get { return _tasks.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void AssignPriority(string providerId, double priority)
        {
            if (_priorityMap.ContainsKey(providerId))
            {
                _priorityMap[providerId] = priority;
            }
            else
            {
                _priorityMap.Add(providerId, priority);
            }
        }

        public void UnassignPriority(string providerId)
        {
            if (_priorityMap.ContainsKey(providerId))
            {
                _priorityMap.Remove(providerId);
            }
        }

        private SubListNode InitNewSubList(string providerId)
        {
            var node = new SubListNode(0, 0, providerId);
            if (_subListNodeHead.Next == null)
            {
                // first one
                _subListNodeHead.Next = node;
            }
            else
            {
                double p = _priorityMap.ContainsKey(providerId) ? _priorityMap[providerId] : double.MinValue;
                // insert by priority
                SubListNode prevNode = _subListNodeHead;
                SubListNode nextNode = _subListNodeHead.Next;
                while (nextNode != null)
                {
                    double cp = _priorityMap.ContainsKey(nextNode.ProviderId) ?
                        _priorityMap[nextNode.ProviderId] : double.MinValue;
                    if (cp < p)
                    {
                        prevNode.Next = node;
                        node.Next = nextNode;
                        break;
                    }
                    prevNode = prevNode.Next;
                    nextNode = nextNode.Next;
                }
            }
            return node;
        }

        private void OffsetFollowingSubListNodes(SubListNode node, int offset)
        {
            SubListNode nextNode = node.Next;
            while (nextNode != null)
            {
                nextNode.Start += offset;
                nextNode = nextNode.Next;
            }
        }

        private void DispatchCollectionChangedEvent(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = CollectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        class SubListNode
        {

            public string ProviderId { get; set; }
            public int Start { get; set; }
            public int Count { get; set; }
            public SubListNode Next;

            public SubListNode(int start, int count, string providerId)
            {
                Start = start;
                Count = count;
                ProviderId = providerId; 
            }
        }
    }

    public class TaskDataListDesignTime : TaskDataList
    {
        public TaskDataListDesignTime() : base()
        {
            base.Add(new TaskDataDesignTime(1));
            base.Add(new TaskDataDesignTime(2));
            base.Add(new TaskDataDesignTime(3));
        }
    }

}
