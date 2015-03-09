using System.Collections.Generic;
using LauncherZLib.Launcher;

namespace LauncherZLib.Plugin
{

    /// <summary>
    /// <para>Describes a plugin for LauncherZ.</para>
    /// <para>Implementation guidelines:</para>
    /// <list type="bullet">
    ///     <item>
    ///         <description>
    ///             All methods implementation should be synchronous.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <description>
    ///             
    ///         </description>
    ///     </item>
    /// </list>
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Activates the plugin.
        /// </summary>
        /// <param name="pluginContext"></param>
        void Activate(IPluginContext pluginContext);

        /// <summary>
        /// Deactivates the plugin.
        /// </summary>
        /// <param name="pluginContext"></param>
        void Deactivate(IPluginContext pluginContext);

        /// <summary>
        /// Queries available commands.
        /// </summary>
        /// <param name="query"></param>
        /// <returns>A collection of commands. You should return an empty collection if no
        /// commands are available.</returns>
        IEnumerable<LauncherData> Query(LauncherQuery query);
        

    }
}
