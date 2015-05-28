using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LauncherZLib.Event;
using LauncherZLib.Event.PluginInternal;
using LauncherZLib.Plugin;
using LauncherZLib.Utils;

namespace LauncherZLib.Launcher
{
    public sealed class QueryDistributor
    {

        private readonly PluginManager _pluginManager;
        private int _maxResults;

        private LauncherQuery _currentQuery;

        public QueryDistributor(PluginManager manager, int maxResults)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            _pluginManager = manager;
            _maxResults = maxResults;
            // handles events from plugins
            _pluginManager.RegisterPluginEventHandler(this);
        }

        public event EventHandler<ResultUpdatedEventArgs> ResultUpdate;

        public event EventHandler QueryReset;

        public int MaxResults
        {
            get { return _maxResults; }
            set { _maxResults = value > 0 ? value : 1; }
        }

        public LauncherQuery CurrentQuery { get { return _currentQuery; } }


        public void DistributeQuery(LauncherQuery query)
        {
            _currentQuery = query;
            var resultsSync = new List<TaggedObject<LauncherData>>();
#if DEBUG
            var sw = new Stopwatch();
            sw.Start();
#endif
            foreach (var container in _pluginManager.SortedActivePlugins)
            {
                if (resultsSync.Count > _maxResults)
                    break;
                var immResults = container.PluginInstance.Query(query);
                resultsSync.AddRange(immResults.Select(launcherData => new TaggedObject<LauncherData>(container.PluginId, launcherData)));
            }
#if DEBUG
            sw.Stop();
            Trace.WriteLine(string.Format("Total query time: {0}ms.", sw.ElapsedMilliseconds));
#endif
            if (resultsSync.Count > 0)
            {
                RaiseResultUpdateEvent(resultsSync);
            }
        }

        public void ClearCurrentQuery()
        {
            _currentQuery = null;
            var handlers = QueryReset;
            if (handlers != null)
                handlers(this, EventArgs.Empty);
        }

        private void RaiseResultUpdateEvent(IEnumerable<TaggedObject<LauncherData>> results)
        {
            EventHandler<ResultUpdatedEventArgs> handler = ResultUpdate;
            if (handler != null)
                handler(this, new ResultUpdatedEventArgs(results));
        }

        #region Event Handling

        [SubscribeEvent]
        public void LauncherResultUpdateHandler(QueryResultUpdateEventI e)
        {
            if (_currentQuery == null || _currentQuery.QueryId != e.BaseEvent.QueryId || e.BaseEvent.Results == null)
                return;
            // assign plugin id and raise update event
            RaiseResultUpdateEvent(e.BaseEvent.Results.Select(l => new TaggedObject<LauncherData>(e.SourceId, l)));
        }

        #endregion
    }

    public class ResultUpdatedEventArgs : EventArgs
    {
        public IEnumerable<TaggedObject<LauncherData>> Updates { get; private set; }

        public ResultUpdatedEventArgs(IEnumerable<TaggedObject<LauncherData>> updates)
        {
            Updates = updates ?? Enumerable.Empty<TaggedObject<LauncherData>>();
        }
    }

}
