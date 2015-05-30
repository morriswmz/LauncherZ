using System;
using LauncherZLib.Event;
using LauncherZLib.I18N;
using LauncherZLib.Utils;

namespace LauncherZLib.Plugin.Service
{
    /// <summary>
    /// Provides a collection of essential plugin services.
    /// </summary>
    public sealed class EssentialPluginServices
    {
        /// <summary>
        /// Retrieves plugin information provider, which enables access to various plugin information.
        /// </summary>
        public IPluginInfoProvider PluginInfo { get; private set; }

        /// <summary>
        /// Retrieves the localization dictionary.
        /// </summary>
        public ILocalizationDictionary Localization { get; private set; }

        /// <summary>
        /// Retrieves the logger of this plugin.
        /// </summary>
        public ILogger Logger { get; private set; }
        
        /// <summary>
        /// Retrieves the event bus of this plugin.
        /// </summary>
        public IEventBus EventBus { get; private set; }
        
        /// <summary>
        /// Retrieves the dispatcher service.
        /// You can invoke your method asynchronously over the main UI thread via this service. 
        /// </summary>
        public IDispatcherService Dispatcher { get; private set; }
        
        public EssentialPluginServices(IPluginInfoProvider pluginInfo, ILocalizationDictionary localization, ILogger logger, IEventBus eventBus, IDispatcherService dispatcher)
        {
            if (pluginInfo == null)
                throw new ArgumentNullException("pluginInfo");
            if (localization == null)
                throw new ArgumentNullException("localization");
            if (logger == null)
                throw new ArgumentNullException("logger");
            if (eventBus==null)
                throw new ArgumentNullException("eventBus");
            if (dispatcher == null)
                throw new ArgumentNullException("dispatcher");

            PluginInfo = pluginInfo;
            Localization = localization;
            Logger = logger;
            EventBus = eventBus;
            Dispatcher = dispatcher;
        }
    }
}
