using System;
using System.IO;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Utils;

namespace LauncherZLib.Plugin.Loader
{
    public sealed class PluginLoader
    {

        private ILogger _logger;

        /// <summary>
        /// Creates a new plugin loader.
        /// </summary>
        public PluginLoader(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            _logger = logger;
        }

        public IPlugin Load(PluginDiscoveryInfo pdi)
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
                _logger.Warning(string.Format("Assembly file \"{0}\" does not exist. Skipped.", pdi.AssemblyPath));
                return null;
            }
            var pluginInstance = Activator.CreateInstanceFrom(pdi.AssemblyPath, pdi.PluginClass).Unwrap() as IPlugin;
            return pluginInstance;
        }

    }
}
