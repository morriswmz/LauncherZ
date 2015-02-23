using LauncherZLib.Event;
using LauncherZLib.I18N;
using LauncherZLib.Utils;

namespace LauncherZLib.Plugin
{
    /// <summary>
    /// Provides useful methods to plugins.
    /// </summary>
    public interface IPluginContext
    {
        /// <summary>
        /// Gets the id of this plugins.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the EventBus created for this plugin.
        /// </summary>
        IEventBus EventBus { get; }

        /// <summary>
        /// Gets the logger created for this plugin.
        /// </summary>
        ILogger Logger { get; }

        /// <summary>
        /// Gets the LocalizationDictionary created for this plugin.
        /// </summary>
        LocalizationDictionary Localization { get; }

        /// <summary>
        /// Gets the source directory of this plugin.
        /// </summary>
        string SourceDirectory { get; }

        /// <summary>
        /// Gets the suggested directory to store plugin data.
        /// </summary>
        string SuggestedDataDirectory { get; }

    }
}
