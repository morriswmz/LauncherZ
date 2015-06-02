using System.Collections.Generic;
using System.ComponentModel;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Plugin.Template;

namespace LauncherZ.Plugin
{
    [Plugin("LZInternals", Authors = "morriswmz", FriendlyName = "LauncherZ Internal Tools", Version = "1.0.0.0")]
    [Description("Provides internal commands.")]
    sealed class InternalsPlugin : EmptyPlugin
    {
        private CommandModule<BasicCommandHandler> _commandModule;

        public override void Activate(IPluginServiceProvider serviceProvider)
        {
            base.Activate(serviceProvider);
            _commandModule = new CommandModule<BasicCommandHandler>(true);
            _commandModule.AddOrUpdateCommandHanlder(new AboutCommandHandler(serviceProvider));
            _commandModule.AddOrUpdateCommandHanlder(new PluginCommandHandler(serviceProvider));
        }

        public override void Deactivate(IPluginServiceProvider serviceProvider)
        {
            _commandModule.RemoveAllCommandHandlers();
            _commandModule = null;
            base.Deactivate(serviceProvider);
        }

        public override IEnumerable<LauncherData> Query(LauncherQuery query)
        {
            return _commandModule.HandleQuery(query);
        }

        public override PostLaunchAction Launch(LauncherData launcherData)
        {
            return _commandModule.HandleLaunch((CommandLauncherData) launcherData);
        }
    }
}
