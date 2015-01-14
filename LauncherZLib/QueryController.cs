using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Threading;
using LauncherZLib.API;
using LauncherZLib.Event;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin;

namespace LauncherZLib
{
    public sealed class QueryController
    {

        private readonly PluginManager _pluginManager;
        private readonly Dispatcher _dispatcher;
        private readonly DispatcherTimer _tickTimer;
        private int _divider = 0;
        private int _maxResults;

        private LauncherList _results;
        private ReadOnlyObservableCollection<LauncherData> _resultsReadOnly;
        private LauncherQuery _currentQuery;
        private Dictionary<string, bool> _asyncCompleteFlags = new Dictionary<string, bool>(); 
        

        public QueryController(PluginManager manager, int maxResults)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            _pluginManager = manager;
            _dispatcher = Dispatcher.CurrentDispatcher;
            _tickTimer = new DispatcherTimer(DispatcherPriority.Normal, _dispatcher);
            _tickTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            _tickTimer.Tick += TickTimer_Tick;
            _maxResults = maxResults;
            _results = new LauncherList(new LauncherDataComparer(manager));
            
            foreach (var pluginContainer in _pluginManager.SortedActivePlugins)
            {
                if (pluginContainer.IsAsync)
                {
                    pluginContainer.AsyncUpdate += Plugin_AsyncUpdate;
                }
            }

            _pluginManager.PluginActivated += PluginManager_PluginActivated;
            _pluginManager.PluginDeactivated += PluginManager_PluginDeactivated;
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
            if (_results.Count > 0)
                _tickTimer.Start();
        }

        public void ClearCurrentQuery()
        {
            _results.Clear();
            _currentQuery = null;
        }

        #region Event Handling

        private void TickTimer_Tick(object sender, EventArgs e)
        {
            if (_results.Count == 0)
            {
                _tickTimer.Stop();
                _divider = 0;
                return;
            }
            _divider++;
            bool tickNormal = _divider%5 == 0;
            bool tickSlow = false;
            if (_divider == 20)
            {
                _divider = 0;
                tickSlow = true;
            }
            foreach (var cmd in _results.Where(cmd => cmd.ExtendedProperties.Tickable))
            {
                bool shouldTick = cmd.ExtendedProperties.CurrentTickRate == TickRate.Fast;
                shouldTick = shouldTick || (tickNormal && cmd.ExtendedProperties.CurrentTickRate == TickRate.Normal);
                shouldTick = shouldTick || (tickSlow && cmd.ExtendedProperties.CurrentTickRate == TickRate.Slow);
                if (shouldTick)
                {
                    _pluginManager.DistributeEventTo(cmd.PluginId, new LauncherTickEvent(cmd));
                }    
            }
        }


        private void PluginManager_PluginDeactivated(object sender, PluginManagerEventArgs e)
        {
            PluginContainer container = _pluginManager.GetPluginContainer(e.PluginId);
            if (container.IsAsync)
            {
                container.AsyncUpdate -= Plugin_AsyncUpdate;
            }
        }

        private void PluginManager_PluginActivated(object sender, PluginManagerEventArgs e)
        {
            PluginContainer container = _pluginManager.GetPluginContainer(e.PluginId);
            if (container.IsAsync)
            {
                container.AsyncUpdate += Plugin_AsyncUpdate;
            }
        }
        
        /// <summary>
        /// Handles asynchrounous results.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Plugin_AsyncUpdate(object sender, AsyncUpdateEventArgs e)
        {
            if (_dispatcher.CheckAccess())
            {
                DoAsyncUpdate((PluginContainer) sender, e);
            }
            else
            {
                _dispatcher.InvokeAsync(() => DoAsyncUpdate((PluginContainer) sender, e));
            }
        }

        private void DoAsyncUpdate(PluginContainer container, AsyncUpdateEventArgs e)
        {
            if (_currentQuery == null || _currentQuery.QueryId != e.QueryId)
                return;
            // assign plugin id;
            foreach (var commandData in e.Results)
            {
                commandData.PluginId = container.Id;
            }
            _results.AddRange(e.Results);
            // check final flag
            if (e.IsFinal)
                _asyncCompleteFlags[container.Id] = true;
            // check timer
            if (!_tickTimer.IsEnabled)
                _tickTimer.Start();
        }

        #endregion

        

        sealed class LauncherDataComparer : IComparer<LauncherData>
        {
            private readonly PluginManager _pluginManager;

            public LauncherDataComparer(PluginManager pluginManager)
            {
                _pluginManager = pluginManager;
            }

            public int Compare(LauncherData x, LauncherData y)
            {
                if (x.Relevance > y.Relevance)
                    return 1;

                if (x.Relevance < y.Relevance)
                    return -1;

                double xp = _pluginManager.GetPriorityOf(x.PluginId);
                double yp = _pluginManager.GetPriorityOf(y.PluginId);
                return xp.CompareTo(yp);
            }
        }
    }

    

}
