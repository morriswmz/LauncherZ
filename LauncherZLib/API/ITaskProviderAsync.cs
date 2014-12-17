using System;
using System.Collections.Generic;
using LauncherZLib.LauncherTask;

namespace LauncherZLib.API
{
    interface ITaskProviderAsync : ITaskProvider
    {

        /// <summary>
        /// Raised when new results are available.
        /// </summary>
        event EventHandler<UpdateEventArgs> Update;

    }

    public class UpdateEventArgs : EventArgs
    {
        public long QueryId { get; private set; }
        public IEnumerable<TaskData> Results { get; private set; }

        public UpdateEventArgs(long queryId, IEnumerable<TaskData> results)
        {
            QueryId = queryId;
            Results = results;
        }
    }

}
