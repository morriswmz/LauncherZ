using System;
using System.Collections.Generic;
using System.IO;
using LauncherZLib.Event;
using LauncherZLib.I18N;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Utils;

namespace LauncherZLib.Plugin.Template
{
    public abstract class EmptyPlugin : IPlugin
    {
        protected IPluginServiceProvider ServiceProvider;
        protected IPluginInfoProvider PluginInfo;
        protected ILogger Logger;
        protected ILocalizationDictionary Localization;
        protected IEventBus EventBus;

        public virtual void Activate(IPluginServiceProvider serviceProvider)
        {
            // setup context
            ServiceProvider = serviceProvider;
            PluginInfo = serviceProvider.GetService<IPluginInfoProvider>();
            Logger = serviceProvider.GetService<ILogger>();
            Localization = serviceProvider.GetService<ILocalizationDictionary>();
            EventBus = serviceProvider.GetService<IEventBus>();
            // prepare working directory
            string suggestedDataDir = PluginInfo.SuggestedPluginDataDirectory;
            if (!Directory.Exists(suggestedDataDir))
            {
                try
                {
                    Directory.CreateDirectory(suggestedDataDir);
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format(
                        "An exception occurred while creating data directory:{0}{1}",
                        Environment.NewLine, ex));
                }
            }
        }

        public abstract void Deactivate(IPluginServiceProvider serviceProvider);

        public abstract IEnumerable<LauncherData> Query(LauncherQuery query);
    }
}
