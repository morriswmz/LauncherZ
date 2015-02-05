using System.Collections.Generic;
using LauncherZLib.API;
using LauncherZLib.Launcher;

namespace LauncherZLib.Event.Plugin
{
    public class QueryResultUpdateEvent : PluginSourcedEvent
    {
        /// <summary>
        /// Gets the query id associated with this update.
        /// </summary>
        public long QueryId { get; private set; }

        /// <summary>
        /// Gets if current update is final for given query id.
        /// If true, furthur updates associated with this query id and plugin will be discarded.
        /// </summary>
        public bool IsFinal { get; private set; }

        /// <summary>
        /// Gets the new results.
        /// </summary>
        public IEnumerable<LauncherData> Results { get; private set; }

        public QueryResultUpdateEvent(IPluginContext context, long queryId, IEnumerable<LauncherData> results,
            bool isFinal)
            : base(context)
        {
            QueryId = queryId;
            Results = results;
            IsFinal = isFinal;
        }

        public QueryResultUpdateEvent(IPluginContext context, long queryId, IEnumerable<LauncherData> results)
            : this(context, queryId, results, false)
        { }
    }
}
