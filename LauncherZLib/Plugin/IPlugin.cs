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
        void Activate(IPluginServiceProvider serviceProvider);

        /// <summary>
        /// Deactivates the plugin.
        /// </summary>
        /// <param name="serviceProvider"></param>
        void Deactivate(IPluginServiceProvider serviceProvider);

        /// <summary>
        /// Queries available commands.
        /// </summary>
        /// <param name="query"></param>
        /// <returns>A collection of commands. You should return an empty collection if no
        /// commands are available.</returns>
        IEnumerable<LauncherData> Query(LauncherQuery query);
        

    }
}
