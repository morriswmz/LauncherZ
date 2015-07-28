using System;
using System.Collections.Generic;
using System.IO;
using LauncherZLib.Event;
using LauncherZLib.I18N;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Utils;

namespace LauncherZLib.Plugin.Modules
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
            PluginInfo = serviceProvider.Essentials.PluginInfo;
            Logger = serviceProvider.Essentials.Logger;
            Localization = serviceProvider.Essentials.Localization;
            EventBus = serviceProvider.Essentials.EventBus;
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
                    Logger.Error(
                        "An exception occurred while creating data directory:{0}{1}",
                        Environment.NewLine, ex);
                }
            }
            // load localization
            LoadLocalizationFiles();
        }

        public virtual void Deactivate(IPluginServiceProvider serviceProvider)
        {
            Localization.Clear();
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

        public virtual PostLaunchAction Launch(LauncherData launcherData, LaunchContext context)
        {
            return PostLaunchAction.Default;
        }

        protected virtual void LoadLocalizationFiles()
        {
            
            var languageFileAttr = GetType().GetCustomAttributes(typeof (LanguageFilesAttribute), false) as LanguageFilesAttribute[];
            if (languageFileAttr != null && languageFileAttr.Length > 0)
            {
                // language files defined in attributes, load them in order
                string[] paths = languageFileAttr[0].LanguageFiles;
                foreach (var path in paths)
                {
                    string fullPath = null;
                    try
                    {
                        // check possible absolute path
                        fullPath = path.IndexOf(":/", StringComparison.Ordinal) < 0
                            ? Path.Combine(PluginInfo.PluginSourceDirectory, path)
                            : path;
                        Localization.LoadLanguageFile(fullPath);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning(
                            "An exception occurred while loading the language file \"{0}\". Details:{1}{2}",
                            fullPath, Environment.NewLine, ex);
                    }
                }
            }
            else
            {
                // no attributes, check default
                string defaultI18NDir = Path.Combine(PluginInfo.PluginSourceDirectory, "I18N");
                if (Directory.Exists(defaultI18NDir))
                {
                    string defaultI18NFile = Path.Combine(defaultI18NDir, PluginInfo.PluginId + ".json");
                    try
                    {
                        Localization.LoadLanguageFile(defaultI18NFile);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning("Unable to automatically load default language file \"{0}\". You may need to manually load language files.", defaultI18NFile);
                    }
                }
            }
        }
    }
}
