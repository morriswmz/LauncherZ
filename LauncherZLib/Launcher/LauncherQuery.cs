using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using LauncherZLib.Utils;

namespace LauncherZLib.Launcher
{
    public sealed class LauncherQuery
    {
        private static long _queryCounter = 0;

        private readonly long _queryId;
        private readonly string _originalInput;
        private readonly ReadOnlyCollection<string> _arguments;

        private LauncherQuery(long queryId, string originalInput, string[] arguments)
        {
            _queryId = queryId;
            _originalInput = originalInput;
            _arguments = Array.AsReadOnly(arguments);
        }

        /// <summary>
        /// Gets the unique query id.
        /// </summary>
        public long QueryId { get { return _queryId; } }

        /// <summary>
        /// Gets the original output.
        /// </summary>
        public string OriginalInput { get { return _originalInput; } }

        /// <summary>
        /// Gets the parse arguments.
        /// </summary>
        public ReadOnlyCollection<string> Arguments { get { return _arguments; } }

        /// <summary>
        /// Creates a new query from given input.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static LauncherQuery Create(string input)
        {
            long newId = Interlocked.Increment(ref _queryCounter);
            return new LauncherQuery(newId, input, ParseArguments(input));
        }

        /// <summary>
        /// Splits original input into arguments by space and double quotes.
        /// Double quotes are used to group words together.
        /// Note: No escapes so double quote literal is not available.
        /// </summary>
        /// <param name="rawInput"></param>
        /// <returns></returns>
        /// <example>
        /// abc      -> ["abc"]
        /// a bc     -> ["a", "bc"]
        /// "a b" c  -> ["a b", "c"]
        /// "a b"" c -> ["a b", " c"] // missing doute quote is added automatically at the end of the string
        /// ""a b""  -> ["a", "b"] // empty arguments are removed
        /// </example>
        public static string[] ParseArguments(string rawInput)
        {
            if (string.IsNullOrWhiteSpace(rawInput))
                return new string[0];

            rawInput = rawInput.Trim();
            int idx = 0, lastIdx = 0, n = rawInput.Length;
            bool beginQuote = false;
            var args = new List<string>();
            while (idx < n)
            {
                char c = rawInput[idx];
                if (char.IsWhiteSpace(c) && !beginQuote)
                {
                    if (idx > lastIdx)
                        args.Add(rawInput.Substring(lastIdx, idx - lastIdx));
                    // consume all white spaces
                    idx++;
                    while (idx < n && char.IsWhiteSpace(rawInput[idx]))
                        idx++;
                    
                    lastIdx = idx;
                }
                else if (c == '\"')
                {
                    if (beginQuote)
                    {
                        // end of quotation
                        beginQuote = false;
                        if (idx > lastIdx)
                            args.Add(rawInput.Substring(lastIdx, idx - lastIdx));
                        lastIdx = idx + 1;
                    }
                    else
                    {
                        // start of quotation
                        beginQuote = true;
                        if (idx > lastIdx)
                            args.Add(rawInput.Substring(lastIdx, idx - lastIdx));
                        lastIdx = idx + 1;
                    }
                    idx++;
                }
                else
                {
                    idx++;
                }
            }
            // last one
            if (idx > lastIdx)
                args.Add(rawInput.Substring(lastIdx, idx - lastIdx));

            return args.ToArray();
        }

    }
}
