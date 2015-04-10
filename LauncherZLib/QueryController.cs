using System;
using System.Collections.Generic;
using System.Linq;
using LauncherZLib.Event;
using LauncherZLib.Event.PluginInternal;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin;

namespace LauncherZLib
{
    public sealed class QueryController
    {

        private readonly PluginManager _pluginManager;
        private int _maxResults;

        private LauncherQuery _currentQuery;

        public QueryController(PluginManager manager, int maxResults)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            _pluginManager = manager;
            _maxResults = maxResults;
            // handles events from plugins
            _pluginManager.RegisterPluginEventHandler(this);
        }

        public event EventHandler<ResultUpdatedEventArgs> ResultUpdate;

        public int MaxResults
        {
            get { return _maxResults; }
            set { _maxResults = value > 0 ? value : 1; }
        }

        public LauncherQuery CurrentQuery { get { return _currentQuery; } }


        public void DistributeQuery(LauncherQuery query)
        {
            var resultsSync = new List<LauncherData>();
            _currentQuery = query;
            foreach (var container in _pluginManager.SortedActivePlugins)
            {
                if (resultsSync.Count > _maxResults)
                    break;
                var immResults = container.PluginInstance.Query(query);
                foreach (var launcherData in immResults)
                {
                    launcherData.PluginId = container.PluginId;
                    resultsSync.Add(launcherData);
                }
            }
            if (resultsSync.Count > 0)
            {
                RaiseResultUpdateEvent(resultsSync);
            }
        }

        public void ClearCurrentQuery()
        {
            _currentQuery = null;
        }

        private void RaiseResultUpdateEvent(IEnumerable<LauncherData> results)
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
            // assign plugin id;
            foreach (var launcherData in e.BaseEvent.Results)
            {
                launcherData.PluginId = e.SourceId;
            }
            RaiseResultUpdateEvent(e.BaseEvent.Results);
        }

        #endregion
    }

    public class ResultUpdatedEventArgs : EventArgs
    {
        public IEnumerable<LauncherData> Updates { get; private set; }

        public ResultUpdatedEventArgs(IEnumerable<LauncherData> updates)
        {
            Updates = updates ?? Enumerable.Empty<LauncherData>();
        }
    }

}
