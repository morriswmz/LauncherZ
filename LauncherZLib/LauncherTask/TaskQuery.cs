using System;
using System.Collections.ObjectModel;

namespace LauncherZLib.LauncherTask
{
    public class TaskQuery
    {
        private readonly long _queryId;
        private readonly string _rawInput;
        private readonly ReadOnlyCollection<string> _arguments;

        public TaskQuery(long queryId, string rawInput, string[] arguments)
        {
            _queryId = queryId;
            _rawInput = rawInput;
            _arguments = Array.AsReadOnly(arguments);
        }

        public long QueryId { get { return _queryId; } }

        public string RawInput { get { return _rawInput; } }

        public ReadOnlyCollection<string> Arguments { get { return _arguments; } } 

    }
}
