using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LauncherZLib.Launcher
{
    /// <summary>
    /// Manages lauch history.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class LaunchHistoryManager
    {
        [JsonProperty("Recent")]
        private LinkedList<string> _recent = new LinkedList<string>();
        [JsonProperty("Top")]
        private List<Tuple<string, int>> _top = new List<Tuple<string, int>>();

        private int _maxHistoryCount = 10;

        public int MaxHistoryCount
        {
            get { return _maxHistoryCount; }
            set
            {
                _maxHistoryCount = Math.Min(Math.Max(1, value), 2048);
                TruncateHistory();
            }
        }

        public IEnumerator<string> GetRecentEnumerator()
        {
            return _recent.GetEnumerator();
        }

        public void PushHistory(string input)
        {
            
        }

        private void TruncateHistory()
        {
            
        }
        
    }
}
