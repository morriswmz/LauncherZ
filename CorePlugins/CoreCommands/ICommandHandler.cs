using System.Collections.Generic;
using LauncherZLib.API;
using LauncherZLib.Event.Launcher;
using LauncherZLib.Launcher;

namespace CorePlugins.CoreCommands
{
    public interface ICommandHandler
    {

        string CommandName { get; }

        IEnumerable<LauncherData> HandleQuery(LauncherQuery query, IPluginContext context);

        void HandleTick(LauncherTickEvent e, IPluginContext context);

        void HandleExecute(LauncherExecutedEvent e, IPluginContext context);

    }
}
