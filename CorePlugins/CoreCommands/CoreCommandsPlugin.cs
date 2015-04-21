using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using CorePlugins.CoreCommands.Commands;
using LauncherZLib.Event;
using LauncherZLib.Event.Launcher;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Plugin.Template;

namespace CorePlugins.CoreCommands
{
    [Plugin("LZCoreCommands", FriendlyName = "LauncherZ Core Commands", Authors = "morriswmz", Version = "0.1.0.0")]
    [Description("Provides basic commands.")]
    public class CoreCommandsPlugin : EmptyPlugin
    {

        private readonly CommandModule<CoreCommandHandler> _commandModel = new CommandModule<CoreCommandHandler>(false); 

        public override void Activate(IPluginServiceProvider serviceProvider)
        {
            base.Activate(serviceProvider);
            Localization.LoadLanguageFile(
                Path.Combine(PluginInfo.PluginSourceDirectory, @"I18N\CoreCommandsStrings.json"));
            AddCommandHandlers();
            EventBus.Register(this);
        }

        public override void Deactivate(IPluginServiceProvider serviceProvider)
        {
            EventBus.Unregister(this);
            _commandModel.RemoveAllCommandHandlers();
        }

        protected void AddCommandHandlers()
        {
            _commandModel.AddOrUpdateCommandHanlder(new CpuCommandHandler(ServiceProvider));
            _commandModel.AddOrUpdateCommandHanlder(new ExitCommandHandler(ServiceProvider));
            _commandModel.AddOrUpdateCommandHanlder(new IpCommandHandler(ServiceProvider));
        }

        public override IEnumerable<LauncherData> Query(LauncherQuery query)
        {
            return _commandModel.HandleQuery(query);
        }

        public override PostLaunchAction Launch(LauncherData launcherData)
        {
            // the casting should be aways successful.
            return _commandModel.HandleLaunch((CommandLauncherData) launcherData);
        }

        [SubscribeEvent]
        public void LauncherTickEventHandler(LauncherTickEvent e)
        {
            var handler = _commandModel.GetCommandHandler((CommandLauncherData) e.LauncherData);
            if (handler != null)
                handler.HandleTick((CommandLauncherData) e.LauncherData);
        }

        [SubscribeEvent]
        public void LauncherSelectedEventHandler(LauncherSelectedEvent e)
        {
            var handler = _commandModel.GetCommandHandler((CommandLauncherData) e.LauncherData);
            if (handler != null)
                handler.HandleSelection((CommandLauncherData) e.LauncherData);
        }

        [SubscribeEvent]
        public void LauncherDeselectedEventHandler(LauncherDeselectedEvent e)
        {
            var handler = _commandModel.GetCommandHandler((CommandLauncherData) e.LauncherData);
            if (handler != null)
                handler.HandleDeselection((CommandLauncherData) e.LauncherData);
        }
    
    }
}
