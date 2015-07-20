using System;
using System.Collections.Generic;
using System.Reflection;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Modules;
using LauncherZLib.Plugin.Service;

namespace LauncherZ.Plugin
{
    class AboutCommandHandler : BasicCommandHandler
    {
        public AboutCommandHandler(IPluginServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string CommandName
        {
            get { return "lz-about"; }
        }

        public override IEnumerable<LauncherData> HandleQuery(LauncherQuery query)
        {
            return new LauncherData[]
            {
                new LauncherData(1.0)
                {
                    Title = ServiceProvider.Essentials.Localization["AboutCommandTitle"],
                    Description = string.Format(ServiceProvider.Essentials.Localization["AboutCommandDescription"], "morriswmz", Assembly.GetEntryAssembly().GetName().Version)
                }
            };
        }
    }
}
