using LauncherZLib.Launcher;

namespace LauncherZLib.Plugin.Template
{
    public class CommandLauncherData : LauncherData
    {
        /// <summary>
        /// Gets the arguments for this command.
        /// </summary>
        public ArgumentCollection CommandArgs { get; private set; }

        public CommandLauncherData(ArgumentCollection commandArgs, double relevance)
            : base(relevance)
        {
            CommandArgs = commandArgs;
        }

    }
}
