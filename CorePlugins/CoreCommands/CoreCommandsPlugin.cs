using System.ComponentModel;
using System.IO;
using CorePlugins.CoreCommands.Commands;
using LauncherZLib.Event;
using LauncherZLib.Event.Launcher;
using LauncherZLib.Plugin;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Plugin.Template;

namespace CorePlugins.CoreCommands
{
    [Plugin("LZCoreCommands", FriendlyName = "LauncherZ Core Commands", Authors = "morriswmz", Version = "0.1.0.0")]
    [Description("Provides basic commands.")]
    public class CoreCommandsPlugin : CommandPlugin
    {

        public override void Activate(IPluginServiceProvider serviceProvider)
        {
            base.Activate(serviceProvider);
            Localization.LoadLanguageFile(
                Path.Combine(PluginInfo.PluginSourceDirectory, @"I18N\CoreCommandsStrings.json"));
            EventBus.Register(this);
        }

        public override void Deactivate(IPluginServiceProvider serviceProvider)
        {
            EventBus.Unregister(this);
            base.Deactivate(serviceProvider);
        }

        protected override void AddCommandHandlers()
        {
            AddCommandHandler(new CpuCommandHandler(ServiceProvider));
            AddCommandHandler(new ExitCommandHandler(ServiceProvider));
            AddCommandHandler(new IpCommandHandler(ServiceProvider));
        }

        [SubscribeEvent]
        public void LauncherTickEventHandler(LauncherTickEvent e)
        {
            var handler = GetCommandHandler(e.LauncherData) as CoreCommandHandler;
            if (handler != null)
                handler.HandleTick((CommandLauncherData) e.LauncherData);
        }

        [SubscribeEvent]
        public void LauncherSelectedEventHandler(LauncherSelectedEvent e)
        {
            var handler = GetCommandHandler(e.LauncherData) as CoreCommandHandler;
            if (handler != null)
                handler.HandleSelection((CommandLauncherData) e.LauncherData);
        }

        [SubscribeEvent]
        public void LauncherDeselectedEventHandler(LauncherDeselectedEvent e)
        {
            var handler = GetCommandHandler(e.LauncherData) as CoreCommandHandler;
            if (handler != null)
                handler.HandleDeselection((CommandLauncherData) e.LauncherData);
        }
    
    }
}
