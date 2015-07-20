using System;
using System.Collections.Generic;
using LauncherZLib.Launcher;

namespace LauncherZLib.Plugin.Modules
{
    /// <summary>
    /// Describes a very basic command handler.
    /// </summary>
    public interface ICommandHandler
    {
        [Obsolete]
        string CommandName { get; }

        /// <summary>
        /// Handles query.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        /// <remarks>
        /// Only return a collection of <see cref="T:LauncherZLib.Launcher.LauncherData"/>s,
        /// but not that of subclasses, since command module will copy the returned data and package
        /// them into <see cref="CommandLauncherData"/>.
        /// </remarks>
        IEnumerable<LauncherData> HandleQuery(LauncherQuery query);

        /// <summary>
        /// Handles launch.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <remarks>
        /// Do not cast the given launcher data into any of its subclasses.
        /// </remarks>
        PostLaunchAction HandleLaunch(LauncherData data, LaunchContext context);

    }
}
