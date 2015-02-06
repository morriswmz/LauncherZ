using System;
using System.Collections.Generic;
using LauncherZLib.Event;
using LauncherZLib.Event.Plugin;
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
        private Dictionary<string, bool> _asyncCompleteFlags = new Dictionary<string, bool>(); 
        

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
                foreach (var commandData in immResults)
                {
                    commandData.PluginId = container.Id;
                }
                _results.AddRange(immResults);
            }
        }

        public void ClearCurrentQuery()
        {
            _results.Clear();
            _currentQuery = null;
        }

        #region Event Handling

        [SubscribeEvent]
        public void LauncherResultUpdateHandler(QueryResultUpdateEvent e)
        {
            if (_currentQuery == null || _currentQuery.QueryId != e.QueryId)
                return;
            // assign plugin id;
            foreach (var commandData in e.Results)
            {
                commandData.PluginId = e.SourcePluginContext.Id;
            }
            _results.AddRange(e.Results);
            // check final flag
            if (e.IsFinal)
                _asyncCompleteFlags[e.SourcePluginContext.Id] = true;
        }

        #endregion
    }

    

}
