using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using LauncherZLib.Event;
using LauncherZLib.I18N;
using LauncherZLib.Plugin.Loader;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Utils;

namespace LauncherZLib.Plugin
{
    public sealed class PluginManager : IAutoCompletionProvider
    {
        // note that id is case insensitive
        private readonly PluginServiceProviderFactory _serviceProviderFactory;
        private readonly Dictionary<string, PluginContainer> _loadedPlugins = new Dictionary<string, PluginContainer>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _deactivatedPluginIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _activePluginIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<PluginContainer> _sortedActiveContainers = new List<PluginContainer>();
        private readonly ReadOnlyCollection<PluginContainer> _sortedActiveContainersReadonly;
        private readonly IEventBus _eventBus = new EventBus();
        private readonly ILoggerProvider _loggerProvider;
        private readonly ILogger _logger;
        private readonly IDispatcherService _dispatcherService;

        private bool _allPluginLoaded = false;
        private bool _pluginSorted = false;

        /// <summary>
        /// Creates a plugin manager.
        /// </summary>
        /// <param name="loggerProvider">Logger provider.</param>
        /// <param name="dispatcherService">
        /// Dispatcher service of the main UI thread.
        /// Asynchrous callbacks will be invoke via this dispatcher service.
        /// </param>
        public PluginManager(ILoggerProvider loggerProvider, IDispatcherService dispatcherService, PluginServiceProviderFactory pspFactory)
        {
            if (loggerProvider == null)
                throw new ArgumentNullException("loggerProvider");
            if (dispatcherService == null)
                throw new ArgumentNullException("dispatcherService");

            _loggerProvider = loggerProvider;
            _logger = _loggerProvider.CreateLogger("PluginManager");
            _dispatcherService = dispatcherService;
            _serviceProviderFactory = pspFactory;
            _sortedActiveContainersReadonly = _sortedActiveContainers.AsReadOnly(); 
        }

        /// <summary>
        /// Retrieves a read-only collection of plugin containers sorted by priority, descending.
        /// </summary>
        public ReadOnlyCollection<PluginContainer> SortedActivePlugins
        {
            get
            {
                if (!_pluginSorted)
                {
                    _sortedActiveContainers.Sort((x, y) => y.PluginPriority.CompareTo(x.PluginPriority));
                    _pluginSorted = true;
                }
                return _sortedActiveContainersReadonly;
            }
        }

        /// <summary>
        /// Gets ids of all loaded plugins.
        /// </summary>
        public string[] LoadedPluginIds
        {
            get { return _loadedPlugins.Keys.ToArray(); }
        }

        /// <summary>
        /// Checks if specific plugin is activated.
        /// </summary>
        /// <param name="pluginId"></param>
        /// <returns></returns>
        public bool IsPluginActivated(string pluginId)
        {
            return _activePluginIds.Contains(pluginId);
        }

        /// <summary>
        /// Retrieves a loaded plugin container by id.
        /// </summary>
        /// <param name="pluginId"></param>
        /// <returns></returns>
        public PluginContainer GetPluginContainer(string pluginId)
        {
            PluginContainer container;
            if (_loadedPlugins.TryGetValue(pluginId, out container))
                return container;
            throw new Exception(string.Format("Plugin with id \"{0}\" is not loaded", pluginId));
        }

        /// <summary>
        /// Activates specific plugin.
        /// </summary>
        /// <param name="pluginId"></param>
        /// <returns>True if successful.</returns>
        public bool Activate(string pluginId)
        {
            PluginContainer container;
            if (!_loadedPlugins.TryGetValue(pluginId, out container))
                throw new Exception(string.Format("Plugin with id \"{0}\" is not loaded.", pluginId));

            if (container.Status == PluginStatus.Activated)
                return true;

            if (container.Status == PluginStatus.Crashed)
                return false;

            try
            {
                // activate
                container.PluginInstance.Activate(container.ServiceProvider);
                container.Status = PluginStatus.Activated;
                _logger.Info(string.Format("Successfully activated {0}.", container));
                // remove from disabled
                _deactivatedPluginIds.Remove(pluginId);
                // rebuild active list
                _activePluginIds.Add(pluginId);
                _sortedActiveContainers.Add(container);
                _pluginSorted = false;
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format(
                    "An exception occured while activitng the plugin {0}. Details: {1}{2}",
                    container, Environment.NewLine, ex));
                container.Status = PluginStatus.Crashed;
                return false;
            }
        }

        /// <summary>
        /// Deactivates specific plugin.
        /// </summary>
        /// <param name="pluginId"></param>
        /// <returns>True if successful.</returns>
        public bool Deactivate(string pluginId)
        {
            PluginContainer container;
            if (!_loadedPlugins.TryGetValue(pluginId, out container))
                throw new Exception(string.Format("Plugin \"{0}\" is not loaded.", pluginId));

            if (container.Status == PluginStatus.Deactivated)
                return true;

            if (container.Status == PluginStatus.Crashed)
                return false;

            try
            {
                // deactivate
                container.PluginInstance.Deactivate(container.ServiceProvider);
                container.Status = PluginStatus.Deactivated;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format(
                    "An exception occured while deactivating the plugin {0}. Details: {1}{2}",
                    container, Environment.NewLine, ex));
                container.Status = PluginStatus.Crashed;
                return false;
            }
            finally
            {
                _logger.Info(string.Format("Deactivated {0} if possible.", container));
                // remove from active anyway
                _activePluginIds.Remove(pluginId);
                _deactivatedPluginIds.Add(pluginId);
                // just remove, no need to sort again
                _sortedActiveContainers.Remove(container);
                // dispatch event
            }
            return true;
        }

        /// <summary>
        /// Deactivates all plugins.
        /// </summary>
        public void DeactivateAll()
        {
            string[] pluginIds = _activePluginIds.ToArray();
            foreach (var pluginId in pluginIds)
            {
                Deactivate(pluginId);
            }
        }

        /// <summary>
        /// Retrieves the priority of specific plugin.
        /// </summary>
        /// <param name="pluginId"></param>
        /// <returns>
        /// Corresponding priority.
        /// If the specified plugin does not exist, 0 will be returned.
        /// </returns>
        public double GetPluginPriority(string pluginId)
        {
            PluginContainer container;
            if (_loadedPlugins.TryGetValue(pluginId, out container))
            {
                return container.PluginPriority;
            }
            return 0.0;
        }

        /// <summary>
        /// Sets the priority of specific plugin.
        /// If the specified plugin does not exist, no action will be taken.
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="priority"></param>
        public void SetPluginPriority(string pluginId, double priority)
        {
            PluginContainer container;
            if (_loadedPlugins.TryGetValue(pluginId, out container))
            {
                container.PluginPriority = priority;
            }
        }

        /// <summary>
        /// Discovers and loads plugins from given folder.
        /// </summary>
        /// <param name="searchPath">Folder to load plugins from.</param>
        /// <param name="dataDirBase">Base directory for plugin data storage.</param>
        public void LoadAllFrom(IEnumerable<string> searchPath, string dataDirBase)
        {
            if (_allPluginLoaded)
                return;

            // discover
            var discoverer = new PluginDiscoverer(_logger);
            discoverer.SearchDirectories.AddRange(searchPath);
            IEnumerable<PluginDiscoveryInfo> candidates = discoverer.DiscoverAll();
            var conflicts = candidates.GroupBy(c => c.Id).Where(g => g.Count() > 1).ToList();
            if (conflicts.Count > 0)
            {
                var sb = new StringBuilder("Conflicting plugin id detected.");
                foreach (var conflict in conflicts)
                {
                    sb.AppendLine(string.Format("The following plugins have the same id \"{0}\"", conflict.Key));
                    foreach (var p in conflict)
                    {
                        sb.AppendLine(string.Format("\"{0}\" in \"{1}\"", p.FriendlyName, p.SourceDirectory));
                    }
                }
                return;
            }
            // load
            var loader = new PluginLoader(_logger);
            foreach (var pdi in candidates)
            {
                IPlugin pluginInstance = null;
                try
                {
                    pluginInstance = loader.Load(pdi);
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format(
                        "An exception occured while loading plugin with id \"{0}\". Details:{1}{2}",
                        pdi.Id, Environment.NewLine, ex
                        ));
                }
                if (pluginInstance != null)
                {
                    IExtendedServiceProvider serviceProvider = _serviceProviderFactory.Create(
                        new Dictionary<Type, object>()
                        {
                            {typeof (IPluginInfoProvider), new StaticPluginInfoProvider(pdi, dataDirBase)},
                            {typeof (ILogger), _loggerProvider.CreateLogger(pdi.Id)},
                            {typeof (IEventBus), new PluginEventBus(pdi.Id, _eventBus, _dispatcherService)},
                            {typeof (ILocalizationDictionary), new LocalizationDictionary()}
                        });
                    var pc = new PluginContainer(pluginInstance, pdi, serviceProvider);
                    _loadedPlugins.Add(pc.PluginId, pc);
                }
            }
            // activate
            foreach (var k in _loadedPlugins.Keys.Except(_deactivatedPluginIds))
            {
                Activate(k);
            }

            _allPluginLoaded = true;
        }
        
        /// <summary>
        /// Broadcasts an event to all activated plugins.
        /// </summary>
        /// <param name="e"></param>
        public void DistributeEvent(EventBase e)
        {
            foreach (var container in _sortedActiveContainers)
            {
                if (container.Status == PluginStatus.Activated)
                container.PluginEventBus.Post(e);
            }
        }

        /// <summary>
        /// Sends an event to specific plugin.
        /// If the plugin specified is not loaded or activated, the event data will be lost.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="pluginId"></param>
        public void DistributeEventTo(string pluginId, EventBase e)
        {
            PluginContainer container;
            if (!_loadedPlugins.TryGetValue(pluginId, out container))
                return;
            if (container.Status == PluginStatus.Activated)
                container.PluginEventBus.Post(e);
        }

        /// <summary>
        /// Registers an event handler for events raised by plugins.
        /// No action will be taken if given handler is already registered.
        /// </summary>
        /// <param name="handler"></param>
        /// <seealso cref="T:LauncherZLib.Event.EventBus"/>
        public void RegisterPluginEventHandler(object handler)
        {
            _eventBus.Register(handler);
        }

        /// <summary>
        /// Removes an event handler for events raised by plugins.
        /// No action will be taken if given handler does not exist.
        /// </summary>
        /// <param name="handler"></param>
        /// <seealso cref="T:LauncherZLib.Event.EventBus"/>
        public void RemovePluginEventHandler(object handler)
        {
            _eventBus.Unregister(handler);
        }

        private void PluginCrashHandler(string pluginId, string friendlyMsg)
        {
            PluginContainer container;
            if (!_loadedPlugins.TryGetValue(pluginId, out container))
            {
                _logger.Severe(string.Format(
                    "Plugin with id \"{0}\" reported a crash but was never loaded. This is impossible!.",
                    pluginId));
                return;
            }

            // make sure event is raised on main UI thread.
            _dispatcherService.InvokeAsync(() =>
            {
                _logger.Error(string.Format(
                    "Plugin {0} crashed with message: {1}", container, friendlyMsg));
                // update collection
                _activePluginIds.Remove(pluginId);
                _sortedActiveContainers.Remove(container);
                _deactivatedPluginIds.Add(pluginId);
                _logger.Info(string.Format("Deactivated {0} if possible.", container));
            });
        }

       
        public IEnumerable<string> GetAutoCompletions(string context, int limit)
        {
            throw new NotImplementedException();
        }

    }

}
