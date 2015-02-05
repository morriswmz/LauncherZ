using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LauncherZLib.API;
using LauncherZLib.Event;
using LauncherZLib.Event.Launcher;
using LauncherZLib.Launcher;

namespace CorePlugins.CoreCommands
{
    public class ExitCommandHandler : ICommandHandler
    {

        public string CommandName { get { return "EXIT"; } }

        public IEnumerable<LauncherData> HandleQuery(LauncherQuery query, IPluginContext context)
        {
            return new LauncherData[]
            {
                new LauncherData(
                    context.Localization["ExitCommandTitle"],
                    context.Localization["ExitCommandDescription"],
                    "LauncherZ|IconGear", 1.0,
                    new CommandExtendedProperties(false, query.Arguments))
            };
        }

        public void HandleTick(LauncherTickEvent e, IPluginContext context)
        {
            throw new NotSupportedException();
        }

        public void HandleExecute(LauncherExecutedEvent e, IPluginContext context)
        {
            Application.Current.Shutdown();
        }

    }
}
