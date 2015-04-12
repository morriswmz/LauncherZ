using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using CorePlugins.CoreCommands.Commands;
using LauncherZLib.Event;
using LauncherZLib.Event.Launcher;
using LauncherZLib.I18N;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Plugin.Template;

namespace CorePlugins.CoreCommands
{
    [Plugin("LZCoreCommands", FriendlyName = "LauncherZ Core Commands", Authors = "morriswmz", Version = "0.1.0.0")]
    [Description("Provides basic commands.")]
    public class CoreCommandsPlugin : CommandPlugin<CoreCommandsConfig>
    {

        public override void Activate(IPluginServiceProvider serviceProvider)
        {
            base.Activate(serviceProvider);
            Localization.LoadLanguageFile(
                Path.Combine(PluginInfo.PluginSourceDirectory, @"I18N\CoreCommandsStrings.json"));
        }

        protected override CoreCommandsConfig CreateDefaultConfiguration()
        {
            return new CoreCommandsConfig();
        }

        protected override void AddCommandHandlers()
        {
            AddCommandHandler(new CpuCommandHandler(ServiceProvider));
            AddCommandHandler(new ExitCommandHandler(ServiceProvider));
            AddCommandHandler(new IpCommandHandler(ServiceProvider));
        }
    
    }
}
