using System.Collections.Generic;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Service;

namespace LauncherZLib.Plugin.Modules
{
    public abstract class BasicCommandHandler : ICommandHandler
    {
        protected IPluginServiceProvider ServiceProvider;

        protected BasicCommandHandler(IPluginServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public abstract string CommandName { get; }

        public abstract IEnumerable<LauncherData> HandleQuery(LauncherQuery query);

        public virtual PostLaunchAction HandleLaunch(LauncherData data, LaunchContext context)
        {
            return PostLaunchAction.DoNothing;
        }


    }
}
