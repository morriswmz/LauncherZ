using System.Collections.Generic;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Service;

namespace LauncherZLib.Plugin
{

    /// <summary>
    /// <para>Describes a plugin for LauncherZ.</para>
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Activates the plugin.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <remarks>
        /// <para>Only access LauncherZApp related resources after this method is invoked.</para>
        /// <para>In case of bad things, you may throw exceptions to terminate the process and your plugin
        /// will not be activated.</para>
        /// </remarks>
        void Activate(IPluginServiceProvider serviceProvider);

        /// <summary>
        /// Deactivates the plugin.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <remarks>
        /// <para>Avoid accessing LauncherZApp related resources after this method is invoked.</para>
        /// <para>Avoid throwing exceptions in this method.</para>
        /// </remarks>
        void Deactivate(IPluginServiceProvider serviceProvider);

        /// <summary>
        /// Queries available commands.
        /// </summary>
        /// <param name="query"></param>
        /// <returns>A collection of commands. You should return an empty collection if no
        /// commands are available.</returns>
        IEnumerable<LauncherData> Query(LauncherQuery query);

        /// <summary>
        /// Launch!
        /// </summary>
        /// <param name="launcherData"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        PostLaunchAction Launch(LauncherData launcherData, LaunchContext context);
    }
}
