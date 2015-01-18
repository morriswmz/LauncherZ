using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.Launcher;

namespace CorePlugins.CoreCommands
{
    public class CommandExtendedProperties : LauncherExtendedProperties
    {

        public string[] Arguments { get; private set; }

        public CommandExtendedProperties(bool tickable, string[] arguments) : base(tickable)
        {
            Arguments = arguments;
        }

        public CommandExtendedProperties(bool tickable, TickRate rate, string[] arguments) : base(tickable, rate)
        {
            Arguments = arguments;
        }
    }
}
