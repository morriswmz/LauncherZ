using System;
using System.Collections.Generic;
using LauncherZLib.Event;
using LauncherZLib.Event.Launcher;
using LauncherZLib.Event.PluginInternal;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin;

namespace LauncherZLib
{
    public sealed class QueryController
    {

        private readonly PluginManager _pluginManager;
        private int _maxResults;

        private LauncherList _results;
        private LauncherQuery _currentQuery;

        public QueryController(PluginManager manager, int maxResults)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            _pluginManager = manager;
            _maxResults = maxResults;
            _results = new LauncherList(new LauncherDataComparer(manager));
            
            // handles events from plugins
            _pluginManager.RegisterPluginEventHandler(this);
        }

        public LauncherQuery CurrentQuery { get { return _currentQuery; } }

        public LauncherList Results { get { return _results; } }

        public void DistributeQuery(LauncherQuery query)
        {
            _results.Clear();
            _currentQuery = query;
            foreach (var container in _pluginManager.SortedActivePlugins)
            {
                if (_results.Count > _maxResults)
                    break;
                // assign plugin id
                var immResults = container.Query(query);
                foreach (var launcherData in immResults)
                {
                    launcherData.PluginId = container.Id;
                    _results.Add(launcherData);
                    _pluginManager.DistributeEventTo(container.Id, new LauncherAddedEvent(launcherData));
                }
            }
        }

        public void ClearCurrentQuery()
        {
            var removed = new LauncherData[_results.Count];
            _results.CopyTo(removed, 0);
            _results.Clear();
            // post events
            foreach (var launcherData in removed)
            {
                _pluginManager.DistributeEventTo(launcherData.PluginId, new LauncherRemovedEvent(launcherData));
            }
            _currentQuery = null;
        }

        #region Event Handling

        [SubscribeEvent]
        public void LauncherResultUpdateHandler(QueryResultUpdateEventI e)
        {
            if (_currentQuery == null || _currentQuery.QueryId != e.BaseEvent.QueryId)
                return;
            // assign plugin id;
            foreach (var commandData in e.BaseEvent.Results)
            {
                commandData.PluginId = e.SourceId;
            }
            _results.AddRange(e.BaseEvent.Results);
        }

        #endregion
    }

    

}
