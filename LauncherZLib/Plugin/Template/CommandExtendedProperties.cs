using System;
using LauncherZLib.Launcher;

namespace LauncherZLib.Plugin.Template
{
    public class CommandExtendedProperties : LauncherExtendedProperties
    {

        public ArgumentCollection CommandArgs { get; private set; }

        public CommandExtendedProperties(ArgumentCollection commandArgs)
            : this(commandArgs, false, TickRate.Normal)
        {

        }

        public CommandExtendedProperties(ArgumentCollection commandArgs, bool tickable, TickRate rate)
            : base(tickable, rate)
        {
            if (commandArgs == null)
                throw new ArgumentNullException("commandArgs");
            CommandArgs = commandArgs;
        }
    }
}
