using System.Collections.Generic;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Service;

namespace LauncherZLib.Plugin.Template
{
    public abstract class BasicCommandHandler :ICommandHandler
    {
        protected IPluginServiceProvider ServiceProvider;

        protected BasicCommandHandler(IPluginServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public abstract string CommandName { get; }

        public abstract IEnumerable<CommandLauncherData> HandleQuery(LauncherQuery query);

        public abstract PostLaunchAction HandleLaunch(CommandLauncherData cmdData);
    }
}
