using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.Event;
using LauncherZLib.Launcher;

namespace CorePlugins.CoreCommands
{
    public interface ICommandHandler
    {

        string CommandName { get; }

        IEnumerable<LauncherData> HandleQuery(LauncherQuery query);

        void HandleTick(LauncherTickEvent e);

        void HandleExecute(LauncherExecutedEvent e);

    }
}
