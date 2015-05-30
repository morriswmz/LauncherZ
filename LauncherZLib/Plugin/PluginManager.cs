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
    // todo: plugin manager should support register/unregister plugin on the fly. load method should be removed
    public sealed class PluginManager : IAutoCompletionProvider
    {
        // note that id is case insensitive
        private readonly Dictionary<string, PluginEntry> _loadedPlugins = new Dictionary<string, PluginEntry>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _deactivatedPluginIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _activePluginIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<PluginContainer> _sortedActiveContainers = new List<PluginContainer>();
        private readonly ReadOnlyCollection<PluginContainer> _sortedActiveContainersReadonly;
        private readonly ILogger _logger;

        private bool _pluginSorted = false;

        /// <summary>
        /// Creates a plugin manager.
        /// </summary>
        /// <param name="logger">Logger.</param>
        public PluginManager(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            _logger = logger;
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
            PluginEntry entry;
            if (_loadedPlugins.TryGetValue(pluginId, out entry))
                return entry.Container;
            throw new Exception(string.Format("Plugin with id \"{0}\" is not loaded", pluginId));
        }

        public void Add(PluginContainer plugin)
        {
            var pluginId = plugin.PluginId;
            if (_loadedPlugins.ContainsKey(pluginId))
            {
                throw new Exception(string.Format("A plugin with the same id \"{0}\" already exists.", pluginId));
            }
            _loadedPlugins.Add(pluginId, new PluginEntry(plugin, PluginStatus.Deactivated));
            _deactivatedPluginIds.Add(pluginId);
        }

        public void AddAndActivate(PluginContainer plugin)
        {
            Add(plugin);
            Activate(plugin.PluginId);
        }

        public void Remove(PluginContainer plugin)
        {
            Remove(plugin.PluginId);
        }

        public void Remove(string pluginId)
        {
            if (IsPluginActivated(pluginId))
            {
                Deactivate(pluginId);
            }
            if (!_loadedPlugins.ContainsKey(pluginId))
            {
                _loadedPlugins.Remove(pluginId);
                _deactivatedPluginIds.Remove(pluginId);
            }   
        }

        

        /// <summary>
        /// Activates specific plugin.
        /// </summary>
        /// <param name="pluginId"></param>
        /// <returns>True if successful.</returns>
        public bool Activate(string pluginId)
        {
            PluginEntry entry;
            if (!_loadedPlugins.TryGetValue(pluginId, out entry))
                throw new Exception(string.Format("Plugin with id \"{0}\" is not loaded.", pluginId));

            if (entry.Status == PluginStatus.Activated)
                return true;

            if (entry.Status == PluginStatus.Crashed)
                return false;

            try
            {
                // activate
                entry.Container.PluginInstance.Activate(entry.Container.ServiceProvider);
                entry.Status = PluginStatus.Activated;
                _logger.Info("Successfully activated {0}.", entry.Container);
                // remove from disabled
                _deactivatedPluginIds.Remove(pluginId);
                // rebuild active list
                _pluginSorted = false;
                _activePluginIds.Add(pluginId);
                _sortedActiveContainers.Add(entry.Container);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(
                    "An exception occured while activitng the plugin {0}. Details: {1}{2}",
                    entry.Container, Environment.NewLine, ex);
                entry.Status = PluginStatus.Crashed;
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
            PluginEntry entry;
            if (!_loadedPlugins.TryGetValue(pluginId, out entry))
                throw new Exception(string.Format("Plugin \"{0}\" is not loaded.", pluginId));

            if (entry.Status == PluginStatus.Deactivated)
                return true;

            if (entry.Status == PluginStatus.Crashed)
                return false;

            try
            {
                // deactivate
                _logger.Info("Sending deactivation signal to {0}.", entry.Container);
                entry.Container.PluginInstance.Deactivate(entry.Container.ServiceProvider);
                entry.Status = PluginStatus.Deactivated;
            }
            catch (Exception ex)
            {
                _logger.Error(
                    "An exception occured while deactivating the plugin {0}. Details: {1}{2}",
                    entry.Container, Environment.NewLine, ex);
                entry.Status = PluginStatus.Crashed;
                return false;
            }
            finally
            {
                // remove from active anyway
                _activePluginIds.Remove(pluginId);
                _deactivatedPluginIds.Add(pluginId);
                // just remove, no need to sort again
                _sortedActiveContainers.Remove(entry.Container);
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
            PluginEntry entry;
            return _loadedPlugins.TryGetValue(pluginId, out entry) ? entry.Container.PluginPriority : 0.0;
        }

        /// <summary>
        /// Sets the priority of specific plugin.
        /// If the specified plugin does not exist, no action will be taken.
        /// </summary>
        /// <param name="pluginId"></param>
        /// <param name="priority"></param>
        public void SetPluginPriority(string pluginId, double priority)
        {
            PluginEntry entry;
            if (_loadedPlugins.TryGetValue(pluginId, out entry))
            {
                entry.Container.PluginPriority = priority;
            }
        }
        
        /// <summary>
        /// Broadcasts an event to all activated plugins.
        /// </summary>
        /// <param name="e"></param>
        public void DistributeEvent(EventBase e)
        {
            foreach (var container in _sortedActiveContainers)
            {
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
            PluginEntry entry;
            if (!_loadedPlugins.TryGetValue(pluginId, out entry))
                return;
            if (entry.Status == PluginStatus.Activated)
                entry.Container.PluginEventBus.Post(e);
        }
        
        public IEnumerable<string> GetAutoCompletions(string context, int limit)
        {
            throw new NotImplementedException();
        }

        sealed class PluginEntry
        {
            public PluginEntry(PluginContainer container, PluginStatus status)
            {
                Container = container;
                Status = status;
            }

            public PluginContainer Container { get; set; }
            public PluginStatus Status { get; set; }
        }

    }

}
