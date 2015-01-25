using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using LauncherZLib.API;
using LauncherZLib.Event;
using LauncherZLib.Icon;
using LauncherZLib.Utils;
using Newtonsoft.Json;

namespace LauncherZLib.Plugin
{
    public sealed class PluginManager : IAutoCompletionProvider, IIconLocationResolver
    {
        // note that id is case insensitive
        private readonly Dictionary<string, PluginContainer> _loadedPlugins = new Dictionary<string, PluginContainer>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _deactivatedPluginIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _activePluginIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<PluginContainer> _sortedActiveContainers = new List<PluginContainer>();
        private readonly ILoggerProvider _loggerProvider;
        private readonly ILogger _logger;
        private readonly Dispatcher _dispatcher;

        #region Events

        /// <summary>
        /// Triggered when a plugin is successfully activated.
        /// </summary>
        public event EventHandler<PluginManagerEventArgs> PluginActivated;
        
        /// <summary>
        /// Triggered when a plugin is deactivated.
        /// The deactivation process may be unsuccessful.
        /// The deactivation may be also caused by plugin crash.
        /// </summary>
        public event EventHandler<PluginManagerEventArgs> PluginDeactivated;
        
        /// <summary>
        /// Triggered when a plugin crashes.
        /// The crash may be reported by the plugin, or occur during activation/deactivation.
        /// </summary>
        public event EventHandler<PluginCrashedEventArgs> PluginCrashed;

        #endregion

        public PluginManager(ILoggerProvider loggerProvider)
        {
            _loggerProvider = loggerProvider;
            _logger = _loggerProvider.CreateLogger("PluginManager");
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        /// <summary>
        /// Retrieves a read-only collection of plugin containers sorted by priority, descending.
        /// </summary>
        public ReadOnlyCollection<PluginContainer> SortedActivePlugins
        {
            get { return _sortedActiveContainers.AsReadOnly(); }
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
                container.Activate();
                _logger.Info(string.Format("Successfully activated {0}.", container));
                // remove from disabled
                _deactivatedPluginIds.Remove(pluginId);
                // rebuild active list
                _activePluginIds.Add(pluginId);
                _sortedActiveContainers.Add(container);
                _sortedActiveContainers.Sort();
                // dispatch event
                DispatchPluginActivatedEvent(pluginId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format(
                    "An exception occured while activitng the plugin {0}. Details: {1}{2}",
                    container, Environment.NewLine, ex));
                DispatchPluginCrashedEvent(pluginId, "An exception occured while activiting the plugin.");
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
                container.Deactivate();
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format(
                    "An exception occured while deactivating the plugin {0}. Details: {1}{2}",
                    container, Environment.NewLine, ex));
                DispatchPluginCrashedEvent(pluginId, "An exception occured while deactivating the plugin.");
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
                DispatchPluginDeactivatedEvent(pluginId);
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
        /// <returns></returns>
        public double GetPriorityOf(string pluginId)
        {
            if (_activePluginIds.Contains(pluginId))
            {
                return _loadedPlugins[pluginId].Priority;
            }
            else
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Discovers and loads plugins from given folder.
        /// </summary>
        /// <param name="searchPath">Folder to load plugins from.</param>
        /// <param name="dataPath">Base folder for plugins to store their data. Each plugin will have a sub-folder
        /// in this folder to store their own data.</param>
        public void LoadAllFrom(string searchPath, string dataPath)
        {
            if (!Directory.Exists(searchPath))
                return;
            // trim trailing slash
            searchPath = searchPath.TrimEnd(Path.DirectorySeparatorChar);
            dataPath = dataPath.TrimEnd(Path.DirectorySeparatorChar);

            string[] directories = Directory.GetDirectories(searchPath);
            var result = new List<PluginContainer>();
            foreach (string dir in directories)
            {
                var infos = new List<PluginInfo>();
                try
                {
                    infos.AddRange(LoadManifestFrom(dir));
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("An exception occurr while reading manifest file from: {0}. Details:{1}{2}", dir, Environment.NewLine, ex));
                }
                if (infos.Count == 0)
                {
                    _logger.Warning(string.Format("Empty manifest file found: {0}", dir));
                    continue;
                }
                foreach (var info in infos)
                {
                    info.SourceDirectory = dir;
                    info.DataDirectory = string.Format("{0}{1}{2}",
                        dataPath, Path.DirectorySeparatorChar, info.Id);
                    try
                    {
                        PluginContainer container = LoadPlugin(info);
                        if (container != null)
                        {
                            _loadedPlugins.Add(container.Id, container);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(string.Format("An exception occurr while loading plugin from: {0}. Details:{1}{2}", dir, Environment.NewLine, ex));
                    }
                }
                
            }

            // activate
            foreach (var pluginId in _loadedPlugins.Keys)
            {
                if (!_deactivatedPluginIds.Contains(pluginId))
                {
                    Activate(pluginId);
                }
            }
            
            _sortedActiveContainers.Sort();
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
                container.EventBus.Post(e);
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
                container.EventBus.Post(e);
        }

        private void DispatchPluginActivatedEvent(string pluginId)
        {
            var handler = PluginActivated;
            if (handler != null)
                handler(this, new PluginManagerEventArgs(pluginId));
        }

        private void DispatchPluginDeactivatedEvent(string pluginId)
        {
            var handler = PluginDeactivated;
            if (handler != null)
                handler(this, new PluginManagerEventArgs(pluginId));
        }

        private void DispatchPluginCrashedEvent(string pluginId, string friendlyMsg)
        {
            var handler = PluginCrashed;
            if (handler != null)
                handler(this, new PluginCrashedEventArgs(pluginId, friendlyMsg));
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

            // make sure event is raised on the same thread where the PluginManager was created.
            _dispatcher.InvokeAsync(() =>
            {
                _logger.Error(string.Format(
                    "Plugin {0} crashed with message: {1}", container, friendlyMsg));
                container.DoCrashCleanup();
                DispatchPluginCrashedEvent(pluginId, friendlyMsg);
                // update collection
                _activePluginIds.Remove(pluginId);
                _sortedActiveContainers.Remove(container);
                _deactivatedPluginIds.Add(pluginId);
                _logger.Info(string.Format("Deactivated {0} if possible.", container));
                DispatchPluginDeactivatedEvent(pluginId);
            });
        }

        private IEnumerable<PluginInfo> LoadManifestFrom(string dir)
        {
            string jsonPath = string.Format("{0}{1}{2}", dir, Path.DirectorySeparatorChar, "manifest.json");
            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException("\"manifest.json\" not found.");
            }

            // read manifest.json
            string json = File.ReadAllText(jsonPath);
            var manifest = JsonConvert.DeserializeObject<PluginManifest>(json);
            return (manifest == null || manifest.Plugins == null) ? Enumerable.Empty<PluginInfo>() : manifest.Plugins;
        }

        private PluginContainer LoadPlugin(PluginInfo info)
        {
            if (string.IsNullOrEmpty(info.Id))
            {
                throw new FormatException("Id is empty or missing.");
            }
            if (info.Id.Equals("LauncherZ", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Id cannot be LauncherZ.");
            }
            if (_loadedPlugins.ContainsKey(info.Id))
            {
                var message = string.Format(
                    "Id \"{0}\" is already used by the following plugin: {1}.",
                    info.Id, _loadedPlugins[info.Id]);
                throw new Exception(message);
            }
            if (!_deactivatedPluginIds.Contains(info.Id))
            {
                if (!info.Id.IsProperId())
                {
                    throw new FormatException(string.Format("Invalid id \"{0}\".", info.Id));
                }

                IPlugin plugin = null;
                if (info.PluginType == PluginType.Assembly)
                {
                    plugin = LoadAssemblyProvider(info);
                }
                else if (info.PluginType == PluginType.Xml)
                {
                    plugin = LoadXmlProvider(info);
                }
                else if (info.PluginType == PluginType.Command)
                {
                    plugin = LoadCommandLineProvider(info);
                }
                else
                {
                    // todo: log this
                }

                // load into container if not null
                if (plugin != null)
                {
                    var container = new PluginContainer(plugin, info, _loggerProvider.CreateLogger(info.Id))
                    {
                        CrashHandler = PluginCrashHandler
                    };
                    return container;
                }
            }
            return null;
        }

        
        private static IPlugin LoadAssemblyProvider(PluginInfo info)
        {
            // normalize
            if (info.Assembly.StartsWith(".\\") || info.Assembly.StartsWith(".//"))
                info.Assembly = info.Assembly.Substring(2);
            string assemblyPath = string.Format("{0}{1}{2}",
                info.SourceDirectory, Path.DirectorySeparatorChar, info.Assembly);
            if (!File.Exists(assemblyPath))
            {
                // todo: log this
                return null;
            }
            return Activator.CreateInstanceFrom(assemblyPath, info.PluginClass).Unwrap() as IPlugin;
        }

        private static IPlugin LoadXmlProvider(PluginInfo info)
        {
            return null;
        }

        private static IPlugin LoadCommandLineProvider(PluginInfo info)
        {
            return null;
        }


        public IEnumerable<string> GetAutoCompletions(string context, int limit)
        {
            throw new NotImplementedException();
        }

        public bool TryResolve(IconLocation location, out string path)
        {
            // if domain is empty, treat path as absolute path
            if (string.IsNullOrEmpty(location.Domain))
            {
                path = location.Path;
                return true;
            }
            // if domain is non-empty
            if (!_loadedPlugins.ContainsKey(location.Domain))
            {
                path = string.Empty;
                return false;
            }
            path = Path.Combine(_loadedPlugins[location.Domain].SourceDirectory, location.Path);
            return true;
        }
    }

}
