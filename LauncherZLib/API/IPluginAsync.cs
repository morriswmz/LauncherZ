using System;
using System.Collections.Generic;
using LauncherZLib.Plugin;

namespace LauncherZLib.API
{

    /// <summary>
    /// Describes a plugin for LauncherZ with asynchronous query.
    /// </summary>
    [Obsolete]
    public interface IPluginAsync : IPlugin
    {

        /// <summary>
        /// Raised when new results are available.
        /// </summary>
        event EventHandler<AsyncUpdateEventArgs> AsyncUpdate;

    }

}
