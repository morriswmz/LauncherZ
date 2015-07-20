using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LauncherZLib.Plugin.Modules.Routing
{
    public partial class UrlRouter<THandler> where THandler : class
    {
        
        /* 
         * regex nodes and "/" node are children of this node.
         *        "/" (root) 
         *            |
         *      ------+-------
         *      |            |
         *  "services/"   "user/"
         *      |            |
         *     ...         ":id"
         */
        private readonly RoutingNode _pathRoot;
        private readonly List<RegexRule> _regexRules; 

        public UrlRouter()
        {
            _pathRoot = new RoutingNode(RoutingNodeType.Regular, "/", null);
            _regexRules = new List<RegexRule>();
        }

        /// <summary>
        /// Adds a regular route, with support for basic parameters.
        /// </summary>
        /// <param name="path">
        /// <para>Escaped absolute path (i.e., starts with slash). Cannot be null.</para>
        /// <para>
        /// Path is split into segments by slash.
        /// </para>
        /// <para>
        /// Parameters can be specified by using colon (e.g., "/home/:username"). This colon should not
        /// be escaped.
        /// If a segment is specified as a parameter, it will match everything (e.g., "/home/:username"
        /// will match "/home/bill" or "/home/morris", but neither "/home/bill/" nor "/home/morris/data".
        /// If a segment is specified as a parameter, it cannot contain anything else (e.g., neither
        /// "/home/user:username" nor "/home/:username:userid" is allowed).
        /// </para>
        /// <para>
        /// In order to prevent ambiguity, you cannot assign two or more different parameter names
        /// for the same type of segments at the same level (e.g., you cannot add both "/home/:username1"
        /// and "/home/:username2"; you cannot add both "/home/:user1/data" and "/home/:user2/data;
        /// however, adding both "/home/:user1" and "/home/:user2/" is fine since the last slash makes
        /// difference).
        /// </para>
        /// <para>
        /// For advanced matching, use regex rule instead.
        /// </para>
        /// </param>
        /// <param name="handler">Handler. Cannot be null.</param>
        public void Add(string path, THandler handler)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            if (handler == null)
                throw new ArgumentNullException("handler");
            if (!path.StartsWith("/"))
                throw new ArgumentException("Absolute path should start with \"/\".");

            if (path.Length == 1 && path[0] == '/')
            {
                // simply "/", update
                _pathRoot.Handler = handler;
                return;
            }
            // split into segments
            string[] segments = UrlHelper.GetSegmentsUnescaped(path);
            // add to routing tree
            RoutingNode curNode = _pathRoot;
            // create possible intermediate nodes
            for (var i = 1; i < segments.Length - 1; i++)
            {
                curNode = CreateOrUpdateSubNode(curNode, segments[i], null);
            }
            // create or update last node
            CreateOrUpdateSubNode(curNode, segments[segments.Length - 1], handler);
        }

        /// <summary>
        /// <para>Adds a regex route.</para>
        /// </summary>
        /// <param name="regex">
        /// <para>Regular expression describing the route.</para>
        /// <para>
        /// Do not escape special charaters in your regular expression, since matching is performed
        /// against unescaped path.
        /// </para>
        /// <para>
        /// If captures groups are unnamed, the corresponding values will be stores in order and can be
        /// access by indexing (e.g., params[1]). The first unnamed parameter value (i.e., params[0])
        /// will always be the whole match.
        /// </para>
        /// <para>
        /// If you name your capture groups, the corresponding values will be also stored in the result
        /// and can be access by its name (e.g., params["myparam"]). The "0" parameter value (i.e.,
        /// params["0"]) will always be the whole match. 
        /// </para>
        /// <para>
        /// If your regular expression allows repetation of capture groups, all captures will be stored
        /// and accessible via indexing. For named captures, only the last match will be accessible by
        /// name. For example, let the regular expression be "/(?&lt;digit&gt;\d)+", then for the path
        /// "/123", params[0], params[1], params[2], params[3], will be "/123", "1", "2", and "3",
        /// respectively; and params["0"], params["digit"] will be "/123", 3, respectively.
        /// </para>
        /// </param>
        /// <param name="handler">Handler. Cannot be null.</param>
        /// <remarks>
        /// Regex route has the lowest priority and relatively the slowest performance.
        /// </remarks>
        public void Add(Regex regex, THandler handler)
        {
            if (regex == null)
                throw new ArgumentNullException("regex");
            if (handler == null)
                throw new ArgumentNullException("handler");

            _regexRules.Add(new RegexRule(regex, handler));
        }

        public RoutingResult Route(Uri url)
        {
            if (url == null)
                throw new ArgumentNullException("url");

            Tuple<string, string[]> t = UrlHelper.GetFullPathAndSegmentsUnescaped(url);
            return Route(t.Item1, t.Item2);
        }

        public RoutingResult Route(string url)
        {
            if (url == null)
                throw new ArgumentNullException("url");

            Tuple<string, string[]> t = UrlHelper.GetFullPathAndSegmentsUnescaped(url);
            return Route(t.Item1, t.Item2);
        }

        /// <summary>
        /// <para>Performs routing.</para>
        /// <para>
        /// Segments follows the same format as <see cref="T:System.Uri"/>.
        /// e.g. "/home/user/data" -> {"/", "home/","user/", "data"}
        /// </para>
        /// <para>
        /// For each level, routing priority follows: specific > unspecific.
        /// In general, specific path > parameter segment > regex. For example,
        ///   "/home/user"
        /// > "/home/:user"
        /// > ^/home/(\w+)$
        /// In this case the last rule is redundant since ":user" matches everything in that segment.
        /// </para>
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="segments"></param>
        /// <returns></returns>
        protected RoutingResult Route(string fullPath, string[] segments)
        {
            return (new RoutingSession(_pathRoot, _regexRules, fullPath, segments)).Route();
        }

        /// <summary>
        /// Create or update a subtree node.
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="uSegment">Segment string, unescaped.</param>
        /// <param name="handler"></param>
        /// <returns>Created node or updated existing node.</returns>
        private RoutingNode CreateOrUpdateSubNode(RoutingNode parentNode, string uSegment, THandler handler)
        {
            RoutingNode existingNode = null;
            var nodeType = RoutingNodeType.Regular;
            if (uSegment.StartsWith(":"))
            {
                // parameter node
                nodeType = RoutingNodeType.NamedParameter;
                string paramName = uSegment.Substring(1);
                if (string.IsNullOrWhiteSpace(paramName))
                {
                    throw new FormatException("Parameter name cannot be empty.");
                }
                // priority : high -> low
                for (var i = parentNode.Children.Count - 1; i >= 0; i--)
                {
                    RoutingNode curNode = parentNode.Children[i];
                    if (curNode.Type == RoutingNodeType.NamedParameter)
                    {
                        var slashMatch = !(curNode.SegmentString.EndsWith("/") ^ uSegment.EndsWith("/"));
                        if (!slashMatch)
                        {
                            continue;
                        }
                        if (curNode.ParameterName.Equals(paramName))
                        {
                            existingNode = curNode;
                            break;
                        }
                        throw new Exception(string.Format(
                            "Parameter name mismatch. You may have specified two or more" +
                            "different parameter names for the same segment. Expected: {0}. New: {1}",
                            curNode.ParameterName, paramName));
                    }
                }
            }
            else
            {
                // regular node
                for (var i = parentNode.Children.Count - 1; i >= 0; i--)
                {
                    RoutingNode curNode = parentNode.Children[i];
                    if (curNode.Type == RoutingNodeType.Regular && curNode.SegmentString.Equals(uSegment))
                    {
                        existingNode = curNode;
                        break;
                    }
                }
            }
            if (existingNode != null)
            {
                if (handler != null)
                {
                    // override existing handler
                    existingNode.Handler = handler;
                }
                return existingNode;
            }
            // need to add a new one
            switch (nodeType)
            {
                case RoutingNodeType.Regular:
                    existingNode = new RoutingNode(RoutingNodeType.Regular, uSegment, handler);
                    // regular node is most specific, append to ensure high priority
                    parentNode.Children.Add(existingNode);
                    break;
                case RoutingNodeType.NamedParameter:
                    existingNode = new RoutingNode(RoutingNodeType.NamedParameter, uSegment, handler);
                    // parameter node comes before regular node, traverse from left and insert
                    // before the first regular node
                    int idx = parentNode.Children.FindLastIndex(n => n.Type == RoutingNodeType.Regular);
                    if (idx >= 0)
                    {
                        parentNode.Children.Insert(idx, existingNode);
                    }
                    else
                    {
                        parentNode.Children.Add(existingNode);
                    }
                    break;
                default:
                    throw new Exception("This should never happen.");
            }
            
            return existingNode;
        }


        public class RoutingResult
        {
            public bool Success { get; private set; }
            public RoutingParameterCollection Parameters { get; private set; }
            public THandler Handler { get; private set; }

            public RoutingResult(bool success, RoutingParameterCollection parameters, THandler handler)
            {
                Success = success;
                Parameters = parameters;
                Handler = handler;
            }

            public override string ToString()
            {
                return string.Format("{{{0}, {1}, \"{2}\"}}", Success ? "Success" : "Fail", Parameters, Handler);
            }
        }

        enum RoutingNodeType
        {
            Regular,
            NamedParameter
        }

        /// <summary>
        /// Internal storage of an routing node.
        /// </summary>
        class RoutingNode
        {
            /// <summary>
            /// Creates a routing node.
            /// </summary>
            /// <param name="type">Node type.</param>
            /// <param name="segement">Full segment string, unescaped, with ending slash if exists.</param>
            /// <param name="handler"></param>
            public RoutingNode(RoutingNodeType type, string segement, THandler handler)
            {
                Type = type;
                switch (type)
                {
                    case RoutingNodeType.Regular:
                        SegmentString = segement;
                        break;
                    case RoutingNodeType.NamedParameter:
                        SegmentString = segement;
                        ParameterName = segement.EndsWith("/")
                            ? segement.Substring(1, segement.Length - 2)
                            : segement.Substring(1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("type");
                }
                Handler = handler;
                Children = new List<RoutingNode>();
            }

            public RoutingNodeType Type { get; private set; }
            /// <summary>
            /// Gets the unescaped full segment string.
            /// </summary>
            public string SegmentString { get; private set; }
            /// <summary>
            /// Gets the unescaped associated parameter name if exists.
            /// </summary>
            public string ParameterName { get; private set; }
            public THandler Handler { get; set; }
            public List<RoutingNode> Children { get; private set; }

            public bool IsLeaf { get { return Children.Count == 0; } }

            public override string ToString()
            {
                return string.Format("{{{0}, \"{1}\", {2}}}",
                    Type,
                    SegmentString,
                    string.Format("{0} {1}", Children.Count, Children.Count > 1 ? "children" : "child"));
            }
        }

        class RegexRule
        {

            private string[] _groupNames;
            
            public RegexRule(Regex matchExpression, THandler handler)
            {
                MatchExpression = matchExpression;
                Handler = handler;
            }

            public Regex MatchExpression { get; private set; }
            public THandler Handler { get; private set; }


            public string[] GetMatchGroupNames()
            {
                return _groupNames ?? (_groupNames = MatchExpression.GetGroupNames());
            }
        }

    }
}
