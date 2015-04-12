using System.Collections.Generic;
using System.Windows;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Plugin.Template;

namespace CorePlugins.CoreCommands.Commands
{
    public sealed class ExitCommandHandler : CoreCommandHandler
    {
        public ExitCommandHandler(IPluginServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string CommandName
        {
            get { return "EXIT"; }
        }

        public override bool SubscribeToEvents
        {
            get { return false; }
        }
        
        public override IEnumerable<LauncherData> HandleQuery(LauncherQuery query)
        {
            return new LauncherData[]
            {
                new CommandLauncherData(query.Arguments, 1.0)
                {
                    Title = Localization["ExitCommandTitle"],
                    Description = Localization["ExitCommandDescription"],
                    IconLocation = "LauncherZ://IconGear"
                }
            };
        }

        public override PostLaunchAction HandleLaunch(LauncherData launcherData, ArgumentCollection arguments)
        {
            Application.Current.Shutdown();
            return PostLaunchAction.Default;
        }

    }
}
