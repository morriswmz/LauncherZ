using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace LauncherZLib.Launcher
{
    /// <summary>
    /// Manages launch history.
    /// </summary>
    /// todo: should also collect plugin usage for reweighting
    [Serializable]
    public sealed class LaunchHistoryManager : ISerializable
    {
        private LinkedList<string> _history;

        private int _maxHistoryCount;

        public LaunchHistoryManager() : this(32)
        {
            
        }

        public LaunchHistoryManager(int maxHistoryCount)
        {
            _maxHistoryCount = maxHistoryCount;
            _history = new LinkedList<string>();
        }

        private LaunchHistoryManager(SerializationInfo info, StreamingContext context)
        {
            var savedHistory = info.GetValue("History", typeof (string[])) as string[];
            _history = savedHistory == null ? new LinkedList<string>() : new LinkedList<string>(savedHistory);
            MaxHistoryCount = info.GetInt32("MaxHistoryCount");
        }

        public int MaxHistoryCount
        {
            get { return _maxHistoryCount; }
            set
            {
                _maxHistoryCount = Math.Min(Math.Max(1, value), 2048);
                TruncateHistory();
            }
        }

        public IEnumerable<string> History
        {
            get { return _history; }
        }

        public void PushHistory(string input, string pluginId)
        {
            if (string.IsNullOrWhiteSpace(input))
                return;
            if (_history.Count > 0 && _history.First.Value != input)
            {
                _history.AddFirst(input);
                TruncateHistory();
            }
            else
            {
                _history.AddFirst(input);
            }
        }

        private void TruncateHistory()
        {
            if (_history.Count <= _maxHistoryCount)
                return;
            while (_history.Count > _maxHistoryCount)
            {
                _history.RemoveLast();
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("MaxHistoryCount", _maxHistoryCount);
            info.AddValue("History", _history.ToArray(), typeof (string[]));
        }
    }
}
