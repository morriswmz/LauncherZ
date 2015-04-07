using System;
using System.Collections.Generic;
using System.Windows;
using LauncherZLib.Event.Launcher;
using LauncherZLib.Launcher;

namespace CorePlugins.CoreCommands
{
    public class ExitCommandHandler : CommandHandler
    {
        public ExitCommandHandler(ICommandPlugin plugin) : base(plugin)
        {
        }

        public override string CommandName { get { return "EXIT"; } }

        public override IEnumerable<LauncherData> HandleQuery(LauncherQuery query)
        {
            return new LauncherData[]
            {
                new LauncherData(
                    Plugin.Localization["ExitCommandTitle"],
                    Plugin.Localization["ExitCommandDescription"],
                    "LauncherZ://IconGear", 1.0,
                    new CommandExtendedProperties(false, query.Arguments))
            };
        }

        public override void HandleExecute(LauncherExecutedEvent e)
        {
            Application.Current.Shutdown();
        }

    }
}
