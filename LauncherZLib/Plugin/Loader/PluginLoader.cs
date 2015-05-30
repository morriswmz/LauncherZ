using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LauncherZLib.Event;
using LauncherZLib.I18N;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Utils;

namespace LauncherZLib.Plugin.Loader
{
    public sealed class PluginLoader
    {

        private readonly ILogger _logger;
        private readonly PluginDiscoverer _discoverer;

        /// <summary>
        /// Creates a new plugin loader.
        /// </summary>
        public PluginLoader(PluginDiscoverer discoverer, ILogger logger)
        {
            if (discoverer == null)
                throw new ArgumentNullException("discoverer");
            if (logger == null)
                throw new ArgumentNullException("logger");
            
            _discoverer = discoverer;
            _logger = logger;
        }

        public IEnumerable<UncontainedPlugin> LoadPlugins(IEnumerable<string> searchPath)
        {
            if (searchPath == null)
                throw new ArgumentNullException("searchPath");

            PluginDiscoveryInfo[] candidates = _discoverer.DiscoverAllIn(searchPath).ToArray();
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
                _logger.Severe(sb.ToString());
                return Enumerable.Empty<UncontainedPlugin>();
            }
            // load
            return candidates.Select(pdi =>
            {
                IPlugin pluginInstance = null;
                try
                {
                    pluginInstance = LoadInstance(pdi);
                }
                catch (Exception ex)
                {
                    _logger.Error(
                        "An exception occured while loading plugin with id \"{0}\". Details:{1}{2}",
                        pdi.Id, Environment.NewLine, ex
                        );
                }
                return pluginInstance == null ? null : new UncontainedPlugin(pluginInstance, pdi);
            }).Where(x => x != null);
        } 
        
        public IPlugin LoadInstance(PluginDiscoveryInfo pdi)
        {
            switch (pdi.Type)
            {
                case PluginType.Assembly:
                    return LoadAssemblyPlugin(pdi);
                default:
                    throw new NotSupportedException(string.Format("Plugin type \"{0}\" is not supported.", pdi.Type));
            }
        }

        private IPlugin LoadAssemblyPlugin(PluginDiscoveryInfo pdi)
        {
            // normalize
            if (!File.Exists(pdi.AssemblyPath))
            {
                _logger.Warning("Assembly file \"{0}\" does not exist. Skipped.", pdi.AssemblyPath);
                return null;
            }
            var pluginInstance = Activator.CreateInstanceFrom(pdi.AssemblyPath, pdi.PluginClass).Unwrap() as IPlugin;
            return pluginInstance;
        }

    }
}
