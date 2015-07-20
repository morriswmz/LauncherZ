using System.Collections.Generic;
using System.Windows;
using LauncherZ.Icon;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Modules;
using LauncherZLib.Plugin.Service;

namespace CorePlugins.CoreCommands.Commands
{
    public sealed class ExitCommandHandler : CoreCommandHandler
    {
        public ExitCommandHandler(IExtendedServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string CommandName
        {
            get { return "EXIT"; }
        }
        
        public override IEnumerable<LauncherData> HandleQuery(LauncherQuery query)
        {
            return new []
            {
                new LauncherData(1.0)
                {
                    Title = Localization["ExitCommandTitle"],
                    Description = Localization["ExitCommandDescription"],
                    IconLocation = LauncherZIconSet.Exit.ToString()
                }
            };
        }

        public override PostLaunchAction HandleLaunch(LauncherData data, LaunchContext context)
        {
            Application.Current.Shutdown();
            return PostLaunchAction.Default;
        }

    }
}
