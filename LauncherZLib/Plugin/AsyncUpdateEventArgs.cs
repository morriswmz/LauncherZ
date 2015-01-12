using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.Launcher;

namespace LauncherZLib.Plugin
{
    public class AsyncUpdateEventArgs : EventArgs
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

        public AsyncUpdateEventArgs(long queryId, IEnumerable<LauncherData> results)
        {
            QueryId = queryId;
            Results = results;
            IsFinal = false;
        }

        public AsyncUpdateEventArgs(long queryId, IEnumerable<LauncherData> results, bool final)
        {
            QueryId = queryId;
            Results = results;
            IsFinal = final;
        }

    }
}
