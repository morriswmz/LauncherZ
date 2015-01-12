using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using LauncherZLib.I18N;

namespace LauncherZLib.API
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

        /// <summary>
        /// Sends crash notification to application in case of unrecoverable exception.
        /// Note:
        /// 1) The message will be sent asynchronously via application's Dispatcher.
        /// 2) You should log the detailed crash report and perform cleanup if necessary.
        /// </summary>
        /// <param name="friendlyMsg"></param>
        void SendCrashNotification(string friendlyMsg);

    }
}
