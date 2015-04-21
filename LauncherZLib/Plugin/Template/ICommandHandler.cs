using System.Collections.Generic;
using LauncherZLib.Launcher;

namespace LauncherZLib.Plugin.Template
{
    /// <summary>
    /// Describes a very basic command handler.
    /// </summary>
    public interface ICommandHandler
    {

        string CommandName { get; }

        IEnumerable<CommandLauncherData> HandleQuery(LauncherQuery query);

        PostLaunchAction HandleLaunch(CommandLauncherData cmdData);

    }
}
