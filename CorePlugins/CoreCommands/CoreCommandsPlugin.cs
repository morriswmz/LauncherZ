using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using CorePlugins.CoreCommands.Commands;
using LauncherZLib.Event;
using LauncherZLib.Event.Launcher;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin;
using LauncherZLib.Plugin.Modules;
using LauncherZLib.Plugin.Service;

namespace CorePlugins.CoreCommands
{
    [Plugin("LZCoreCommands", FriendlyName = "LauncherZ Core Commands", Authors = "morriswmz", Version = "0.1.0.0")]
    [Description("Provides basic commands.")]
    public class CoreCommandsPlugin : EmptyPlugin
    {

        private readonly CommandModule<CoreCommandHandler> _commandModule = new CommandModule<CoreCommandHandler>(false); 

        public override void Activate(IPluginServiceProvider serviceProvider)
        {
            base.Activate(serviceProvider);
            AddCommandHandlers();
            EventBus.Register(this);
        }

        public override void Deactivate(IPluginServiceProvider serviceProvider)
        {
            EventBus.Unregister(this);
            _commandModule.RemoveAllCommandHandlers();
            base.Deactivate(serviceProvider);
        }

        protected void AddCommandHandlers()
        {
            _commandModule.AddOrUpdateCommandHanlder(new CpuCommandHandler(ServiceProvider));
            _commandModule.AddOrUpdateCommandHanlder(new ExitCommandHandler(ServiceProvider));
            _commandModule.AddOrUpdateCommandHanlder(new IpCommandHandler(ServiceProvider));
            _commandModule.AddOrUpdateCommandHanlder(new RunCommandHandler(ServiceProvider));
        }

        public override IEnumerable<LauncherData> Query(LauncherQuery query)
        {
            return _commandModule.HandleQuery(query);
        }

        public override PostLaunchAction Launch(LauncherData launcherData, LaunchContext context)
        {
            // the casting should be aways successful.
            return _commandModule.HandleLaunch(launcherData, context);
        }

        [SubscribeEvent]
        public void LauncherTickEventHandler(LauncherTickEvent e)
        {
            var handler = (CoreCommandHandler) _commandModule.CurrentCommandHandler;
            if (handler != null)
                handler.HandleTick(e.LauncherData);
        }

        [SubscribeEvent]
        public void LauncherSelectedEventHandler(LauncherSelectedEvent e)
        {
            var handler = (CoreCommandHandler)_commandModule.CurrentCommandHandler;
            if (handler != null)
                handler.HandleSelection(e.LauncherData);
        }

        [SubscribeEvent]
        public void LauncherDeselectedEventHandler(LauncherDeselectedEvent e)
        {
            var handler = (CoreCommandHandler)_commandModule.CurrentCommandHandler;
            if (handler != null)
                handler.HandleDeselection(e.LauncherData);
        }
    
    }
}
