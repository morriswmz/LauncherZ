using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.Launcher;

namespace LauncherZLib.Event
{
    public class LauncherResultUpdateEvent : EventBase
    {
        /// <summary>
        /// Gets the query id of new results.
        /// </summary>
        public long QueryId { get; private set; }

        /// <summary>
        /// Gets if current update is final for given query id.
        /// </summary>
        public bool IsFinal { get; private set; }

        /// <summary>
        /// Gets the new results.
        /// </summary>
        public IEnumerable<LauncherData> Results { get; private set; }

        public LauncherResultUpdateEvent(long queryId, IEnumerable<LauncherData> results, bool isFinal)
        {
            QueryId = queryId;
            Results = results;
            IsFinal = isFinal;
        }
    }

    internal class LauncherResultUpdateEventIntl : LauncherResultUpdateEvent
    {
        public string PluginId { get; private set; }

        public LauncherResultUpdateEventIntl(string pluginId, LauncherResultUpdateEvent e)
            : base(e.QueryId, e.Results, e.IsFinal)
        {
            PluginId = pluginId;
        }

    }

}
