using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using LauncherZLib.Event;
using LauncherZLib.Event.Launcher;
using LauncherZLib.Event.Plugin;
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
            if (_results.Count > 0)
                _tickTimer.Start();
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
            // check timer
            if (!_tickTimer.IsEnabled)
                _tickTimer.Start();
        }

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

                double xp = _pluginManager.GetPluginPriority(x.PluginId);
                double yp = _pluginManager.GetPluginPriority(y.PluginId);
                return xp.CompareTo(yp);
            }
        }
    }

    

}
