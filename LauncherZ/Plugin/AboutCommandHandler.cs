using System;
using System.Collections.Generic;
using System.Reflection;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Plugin.Template;

namespace LauncherZ.Plugin
{
    class AboutCommandHandler : BasicCommandHandler
    {
        public AboutCommandHandler(IPluginServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string CommandName
        {
            get { return "about"; }
        }

        public override IEnumerable<CommandLauncherData> HandleQuery(LauncherQuery query)
        {
            return new CommandLauncherData[]
            {
                new CommandLauncherData(query.Arguments, 1.0)
                {
                    Title = ServiceProvider.Essentials.Localization["AboutCommandTitle"],
                    Description = string.Format(ServiceProvider.Essentials.Localization["AboutCommandDescription"], "morriswmz", Assembly.GetEntryAssembly().GetName().Version)
                }
            };
        }

        public override PostLaunchAction HandleLaunch(CommandLauncherData cmdData)
        {
            return PostLaunchAction.DoNothing;
        }
    }
}
