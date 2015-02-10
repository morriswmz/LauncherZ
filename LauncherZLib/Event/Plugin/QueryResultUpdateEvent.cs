using System;
using System.Collections.Generic;
using LauncherZLib.Launcher;

namespace LauncherZLib.Event.Plugin
{
    public class QueryResultUpdateEvent : EventBase
    {
        /// <summary>
        /// Gets the query id associated with this update.
        /// </summary>
        public long QueryId { get; private set; }

        /// <summary>
        /// Gets the new results.
        /// </summary>
        public IEnumerable<LauncherData> Results { get; private set; }

        public QueryResultUpdateEvent(long queryId, IEnumerable<LauncherData> results)
        {
            QueryId = queryId;
            Results = results;
        }

        /// <summary>
        /// The default behavior of this event cannot be prevented.
        /// Attempting to call this method will result in NotSupportedException.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        /// Thrown upon calling this method.
        /// </exception>
        public override void PreventDefault()
        {
            throw new NotSupportedException();
        }
    }
}
