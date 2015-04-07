using LauncherZLib.Launcher;

namespace CorePlugins.CoreCommands
{
    public class CommandExtendedProperties : LauncherExtendedProperties
    {

        public ArgumentCollection Arguments { get; private set; }

        public CommandExtendedProperties(bool tickable, ArgumentCollection arguments)
            : base(tickable)
        {
            Arguments = arguments;
        }

        public CommandExtendedProperties(bool tickable, TickRate rate, ArgumentCollection arguments)
            : base(tickable, rate)
        {
            Arguments = arguments;
        }
    }
}
