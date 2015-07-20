using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using LauncherZLib.Utils;

namespace LauncherZLib.Launcher
{
    public sealed class LauncherQuery
    {
        public static readonly IEnumerable<LauncherData> EmptyResult = Enumerable.Empty<LauncherData>(); 

        private static long _queryCounter = 0;

        private readonly long _queryId;
        private readonly Uri _fullUri;
        private readonly string _targetPluginId;
        private readonly string _originalInput;
        private readonly ArgumentCollection _inputArguments;
        private readonly QueryParameterCollection _parameters;

        /// <summary>
        /// <para>Creates a new query using specified uri.</para>
        /// <para>
        /// In order to supply user input, the uri should be in the format
        /// "[PluginId]://[Path]?input=[UserInput]". Use "launcherz" instead of specific plugin id
        /// to perform broadcast query.
        /// </para>
        /// </summary>
        /// <param name="uri"></param>
        /// <remarks>
        /// <para>
        /// Uri scheme is case insensitive. <b>However, the remaining parts are case sensitive.</b>
        /// </para>
        /// <para>
        /// If there are multiple values for the input parameter (e.g. "input=a&amp;input=b"), only
        /// the last one will be used.
        /// </para>
        /// </remarks>
        public LauncherQuery(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            if (!uri.Scheme.IsProperId())
                throw new FormatException("Supplied uri scheme does not represent a valid plugin id.");

            long newId = Interlocked.Increment(ref _queryCounter);
            _queryId = newId;
            _parameters = new QueryParameterCollection(uri.Query.Length > 0 ? uri.Query.Substring(1) : "");
            _originalInput = _parameters.ContainsKey("input")
                ? _parameters["input"].Last()
                : "";
            _inputArguments = new ArgumentCollection(ParseArguments(_originalInput));
            _targetPluginId = GetTargetPluginId(uri);
            _fullUri = uri;
        }

        /// <summary>
        /// Creates a new query from given input with target plugin id specified.
        /// The uri is given by "[PluginId]://query/?input=[Input]".
        /// </summary>
        /// <param name="input"></param>
        /// <param name="targetPluginId"></param>
        public LauncherQuery(string input, string targetPluginId)
            : this(new Uri(string.Format("{0}://query/?input={1}",
                targetPluginId == null ? "launcherz" : targetPluginId.ToLower(CultureInfo.InvariantCulture),
                Uri.EscapeDataString(input))))
        {
        }

        /// <summary>
        /// Creates a new query from given input.
        /// </summary>
        /// <param name="input"></param>
        public LauncherQuery(string input) : this(input, null) { }

        /// <summary>
        /// <para>Creates a new query based on existing query, with new value for input parameter.</para>
        /// <para>
        /// Only the input parameter is modified and remaining query data are copied. For example,
        /// given query "myplugin://config?name=color&amp;input=red#fragment" and input "blue",
        /// the new query will be "myplugin://config?name=color&amp;input=red#fragment".
        /// </para>
        /// </summary>
        /// <param name="baseQuery"></param>
        /// <param name="newInput"></param>
        /// <remarks>
        /// If the original query uri contains multiple input values (e.g. "input=a&amp;input=b"),
        /// they will be replaced with a single value.
        /// </remarks>
        public LauncherQuery(LauncherQuery baseQuery, string newInput)
        {
            if (baseQuery == null)
                throw new ArgumentNullException("baseQuery");
            if (newInput == null)
                throw new ArgumentNullException("newInput");

            long newId = Interlocked.Increment(ref _queryCounter);
            _queryId = newId;
            _parameters = new QueryParameterCollection(baseQuery.Parameters, new Dictionary<string, IEnumerable<string>>()
            {
                {"input", new string[] {newInput}}
            });
            _originalInput = newInput;
            _inputArguments = new ArgumentCollection(ParseArguments(newInput));
            _targetPluginId = GetTargetPluginId(baseQuery.FullUri);
            // [scheme]://[userinfo]@[authority][path][query][fragment]
            // e.g. launcherz://user@query:80/joke?input=foo#nope
            _fullUri = new Uri(string.Format("{0}://{1}{2}{3}{4}{5}",
                baseQuery.FullUri.Scheme,
                string.IsNullOrEmpty(baseQuery.FullUri.UserInfo) ? "" : baseQuery.FullUri.UserInfo + '@',
                baseQuery.FullUri.Authority,
                baseQuery.FullUri.AbsolutePath,
                _parameters.ToQueryString(true),
                baseQuery.FullUri.Fragment));
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
        /// Gets the parsed arguments.
        /// </summary>
        public ArgumentCollection InputArguments { get { return _inputArguments; } }

        /// <summary>
        /// Gets the parsed parameters.
        /// </summary>
        public QueryParameterCollection Parameters { get { return _parameters; } }

        /// <summary>
        /// If the query targets a specific plugin, gets the id of the target plugin.
        /// Otherwise null is returned.
        /// </summary>
        public string TargetPluginId { get { return _targetPluginId; } }

        /// <summary>
        /// Checks if this query is a broadcast query (i.e. not targeting any specific plugin).
        /// </summary>
        public bool IsBroadcast { get { return _targetPluginId == null; } }

        /// <summary>
        /// Checks if this query is a normal query. A normal query's uri follows
        /// "[PluginId]://query/?[Parameters][Fragment]"
        /// </summary>
        public bool IsNormalQuery { get { return _fullUri.Authority == "query"; } }

        /// <summary>
        /// Gets the full URI.
        /// </summary>
        public Uri FullUri { get { return _fullUri; } }

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
        /// </example>
        /// todo : might need to improve this
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

        private string GetTargetPluginId(Uri uri)
        {
            return uri.Scheme.Equals("launcherz", StringComparison.OrdinalIgnoreCase)
                ? null
                : uri.Scheme;
        }

    }
}
