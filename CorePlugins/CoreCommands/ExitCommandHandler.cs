using System.Collections.Generic;
using System.Windows;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Plugin.Template;

namespace CorePlugins.CoreCommands
{
    public class ExitCommandHandler : CoreCommandHandler
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
                new LauncherData(
                    Localization["ExitCommandTitle"],
                    Localization["ExitCommandDescription"],
                    "LauncherZ://IconGear", 1.0,
                    new CommandExtendedProperties(query.Arguments))
            };
        }

        public override PostLaunchAction HandleLaunch(LauncherData launcherData)
        {
            Application.Current.Shutdown();
            return PostLaunchAction.Default;
        }

    }
}
