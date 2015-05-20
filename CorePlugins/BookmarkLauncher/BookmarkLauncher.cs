using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using CorePlugins.BookmarkLauncher.Chrome;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Plugin.Template;

namespace CorePlugins.BookmarkLauncher
{
    [Plugin("LZBookmarkLauncher", Authors = "morriswmz", FriendlyName = "LauncherZ Bookmark Launcher", Version = "0.1.0.0")]
    [Description("Launches your browser favourites.")]
    public sealed class BookmarkLauncher : EmptyPlugin
    {
        private ConfigModule<BookmarkLauncherConfig> _configModule;
        private BookmarkLibrary _library; 

        public override void Activate(IExtendedServiceProvider serviceProvider)
        {
            base.Activate(serviceProvider);
            _configModule = new ConfigModule<BookmarkLauncherConfig>(Logger, new BookmarkLauncherConfig());
            _configModule.ConfigFilePath = Path.Combine(PluginInfo.SuggestedPluginDataDirectory, "config.json");
            _configModule.LoadConfiguration();
            if (_configModule.Config.BookmarkSources == null || _configModule.Config.BookmarkSources.Length == 0)
            {
                _configModule.Config.BookmarkSources = GetDefaultSources();
            }
            // always initialize a new one here
            _library = new BookmarkLibrary(Path.Combine(PluginInfo.SuggestedPluginDataDirectory, "bookmarks.json"), Logger);
            _library.Readers.Add(new ChromeBookmarkReader());
            _library.Sources.AddRange(_configModule.Config.BookmarkSources);
            _library.LoadCachedBookmarks();
            if (DateTime.Now - _library.LastUpdateTime > new TimeSpan(0, 1, 0, 0))
            {
                _library.ScheduleUpdate();
            }
        }

        public override void Deactivate(IExtendedServiceProvider serviceProvider)
        {
            // save cached bookmarks and clear references to readers/sources
            _library.SaveCachedBookmarks();
            _library.Sources.Clear();
            _library.Readers.Clear();
            _library = null;
            // save configurations
            _configModule.SaveConfiguration();
            _configModule = null;
            base.Deactivate(serviceProvider);
        }

        public override IEnumerable<LauncherData> Query(LauncherQuery query)
        {

            return base.Query(query);
        }

        public override PostLaunchAction Launch(LauncherData launcherData)
        {
            try
            {
                Process.Start(launcherData.StringData);
            }
            catch (Exception ex)
            {
                Logger.Warning(string.Format("Failed to open url \"{0}\". Details: {1}{2}",
                    launcherData.StringData, Environment.NewLine, ex));
            }
            return PostLaunchAction.Default;
        }

        private BookmarkSource[] GetDefaultSources()
        {
            string chromePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Google\Chrome\User Data\Default\Bookmarks");
            return new BookmarkSource[]
            {
                new BookmarkSource("chrome", chromePath), 
            };
        }


    }
}
