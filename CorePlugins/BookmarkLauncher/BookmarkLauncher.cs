using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
        private ConfigModule<BookmarkLauncherConfig> _config;

        public override void Activate(IExtendedServiceProvider serviceProvider)
        {
            base.Activate(serviceProvider);
            _config = new ConfigModule<BookmarkLauncherConfig>(Logger, new BookmarkLauncherConfig());
            _config.ConfigFilePath = Path.Combine(PluginInfo.SuggestedPluginDataDirectory, "config.json");
            _config.LoadConfiguration();
        }

        public override void Deactivate(IExtendedServiceProvider serviceProvider)
        {
            _config.SaveConfiguration();
            _config = null;
            base.Deactivate(serviceProvider);
        }

        public override IEnumerable<LauncherData> Query(LauncherQuery query)
        {
            return base.Query(query);
        }

        public override PostLaunchAction Launch(LauncherData launcherData)
        {
            return base.Launch(launcherData);
        }
    }
}
