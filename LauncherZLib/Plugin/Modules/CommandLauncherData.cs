using LauncherZLib.Launcher;

namespace LauncherZLib.Plugin.Modules
{
    public class CommandLauncherData : LauncherData
    {
        /// <summary>
        /// Gets the arguments for this command.
        /// </summary>
        public ArgumentCollection CommandArgs { get; private set; }

        public CommandLauncherData(LauncherData launcherData, ArgumentCollection args)
            : base(launcherData)
        {
            CommandArgs = args;
        }

    }
}
