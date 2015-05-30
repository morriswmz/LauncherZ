using System;
using System.Collections.Generic;
using LauncherZLib.Event;
using LauncherZLib.I18N;
using LauncherZLib.Plugin.Loader;
using LauncherZLib.Plugin.Service;

namespace LauncherZLib.Plugin
{

    /// <summary>
    /// Contains a plugin, stores its information.
    /// </summary>
    
    public sealed class PluginContainer : IComparable<PluginContainer>
    {

        private double _priority = 0.0;
        private readonly IPluginInfoProvider _pluginInfo;

        public string PluginId { get { return _pluginInfo.PluginId; } }

        public string PluginFriendlyName { get { return _pluginInfo.PluginFriendlyName; } }

        /// <summary>
        /// Gets the localized friendly name.
        /// </summary>
        public string LocalizedPluginFriendlyName
        {
            get { return TranslateWithFallback("PluginFriendlyName", PluginFriendlyName); }
        }

        public IEnumerable<string> PluginAuthors { get { return _pluginInfo.PluginAuthors; } }

        public Version PluginVersion { get { return _pluginInfo.PluginVersion; }}

        public string PluginDescription { get { return _pluginInfo.PluginDescription; } }

        /// <summary>
        /// Gets the localized plugin description.
        /// </summary>
        public string LocalizedPluginDescription
        {
            get { return TranslateWithFallback("PluginDescription", PluginDescription); }
        }

        public string PluginSourceDirectory { get { return _pluginInfo.PluginSourceDirectory; } }

        /// <summary>
        /// Gets or sets the priority of this plugin.
        /// Value is constrained between 0 and 1 inclusive.
        /// </summary>
        public double PluginPriority
        {
            get { return _priority; }
            set
            {
                _priority = double.IsNaN(value) ? 0.0 : Math.Max(0.0, Math.Min(value, 1.0));
            }
        }

        /// <summary>
        /// Gets the plugin instance.
        /// </summary>
        public IPlugin PluginInstance { get; private set; }

        /// <summary>
        /// Gets the associated plugin service provider.
        /// </summary>
        public IPluginServiceProvider ServiceProvider { get; private set; }

        public IEventBus PluginEventBus { get; private set; }

        public PluginContainer(IPlugin pluginInstance, PluginDiscoveryInfo discoveryInfo, IPluginServiceProvider serviceProvider)
        {
            if (pluginInstance == null)
                throw new ArgumentNullException("pluginInstance");
            if (discoveryInfo == null)
                throw new ArgumentNullException("discoveryInfo");
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            ServiceProvider = serviceProvider;
            
            PluginInstance = pluginInstance;
            _pluginInfo = serviceProvider.Essentials.PluginInfo;
            PluginEventBus = serviceProvider.Essentials.EventBus;
        }

        public int CompareTo(PluginContainer other)
        {
            return _priority.CompareTo(other._priority);
        }
        
        private string TranslateWithFallback(string key, string fallback)
        {
            return ServiceProvider.Essentials.Localization.CanTranslate(key)
                ? ServiceProvider.Essentials.Localization[key]
                : fallback;
        }

        public override string ToString()
        {
            return string.Format(
                "{{Name={0}, Version={1}, Authors=[{2}]}}",
                PluginFriendlyName, PluginVersion, string.Join(", ", PluginAuthors));
        }

    }
}
