using System;
using System.Collections.ObjectModel;
using System.Threading;
using LauncherZLib.Utils;

namespace LauncherZLib.Launcher
{
    public sealed class LauncherQuery
    {
        private static long _queryCounter = 0;

        private readonly long _queryId;
        private readonly string _rawInput;
        private readonly ReadOnlyCollection<string> _arguments;

        private LauncherQuery(long queryId, string rawInput, string[] arguments)
        {
            _queryId = queryId;
            _rawInput = rawInput;
            _arguments = Array.AsReadOnly(arguments);
        }

        public long QueryId { get { return _queryId; } }

        public string RawInput { get { return _rawInput; } }

        public ReadOnlyCollection<string> Arguments { get { return _arguments; } }

        public static LauncherQuery Create(string rawInput)
        {
            long newId = Interlocked.Increment(ref _queryCounter);
            return new LauncherQuery(newId, rawInput, StringUtils.ParseArguments(rawInput));
        }

    }
}
