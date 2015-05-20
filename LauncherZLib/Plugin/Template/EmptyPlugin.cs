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
        protected IExtendedServiceProvider ServiceProvider;
        protected IPluginInfoProvider PluginInfo;
        protected ILogger Logger;
        protected ILocalizationDictionary Localization;
        protected IEventBus EventBus;

        public virtual void Activate(IExtendedServiceProvider serviceProvider)
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

        public virtual void Deactivate(IExtendedServiceProvider serviceProvider)
        {
            // clear all references
            EventBus = null;
            Localization = null;
            Logger = null;
            PluginInfo = null;
            ServiceProvider = null;
        }

        public virtual IEnumerable<LauncherData> Query(LauncherQuery query)
        {
            return LauncherQuery.EmptyResult;
        }

        public virtual PostLaunchAction Launch(LauncherData launcherData)
        {
            return PostLaunchAction.Default;
        }
    }
}
