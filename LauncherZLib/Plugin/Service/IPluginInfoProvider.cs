using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LauncherZLib.Plugin.Service
{
    /// <summary>
    /// Provides access to plugin information.
    /// </summary>
    public interface IPluginInfoProvider
    {
        /// <summary>
        /// Gets the plugin id.
        /// </summary>
        string PluginId { get; }

        /// <summary>
        /// Gets the friendy name of the plugin.
        /// </summary>
        string PluginFriendlyName { get; }

        /// <summary>
        /// Gets the authors of the plugin.
        /// </summary>
        IEnumerable<string> PluginAuthors { get; }

        /// <summary>
        /// Gets the version of the plugin.
        /// </summary>
        Version PluginVersion { get; }

        /// <summary>
        /// Gets the description of the plugin.
        /// </summary>
        string PluginDescription { get; }

        /// <summary>
        /// Gets the source directory of the plugin.
        /// </summary>
        string PluginSourceDirectory { get; }

        /// <summary>
        /// Gets the suggested plugin data directory (not necessarily created).
        /// </summary>
        string SuggestedPluginDataDirectory { get; }

    }
}
