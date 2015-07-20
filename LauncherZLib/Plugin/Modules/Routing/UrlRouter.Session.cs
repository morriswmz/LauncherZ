using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LauncherZLib.Plugin.Modules.Routing
{
    public partial class UrlRouter<THandler> where THandler : class 
    {
        class RoutingSession
        {
            private RoutingResult _result;
            private readonly RoutingNode _root;
            private readonly List<RegexRule> _regexRules; 
            private readonly string[] _segments;
            private readonly string _fullPath;
            private readonly List<string> _unnamedParams = new List<string>();
            private readonly Dictionary<string, string> _namedParams = new Dictionary<string, string>();

            public RoutingSession(RoutingNode root, List<RegexRule> regexRules, string fullPath, string[] segments)
            {
                if (root == null)
                    throw new ArgumentNullException("root");
                if (fullPath == null)
                    throw new ArgumentNullException("fullPath");
                if (segments == null)
                    throw new ArgumentNullException("segments");

                _root = root;
                _fullPath = fullPath;
                _segments = segments;
                _regexRules = regexRules;
            }

            public RoutingResult Route()
            {
                if (_result != null)
                {
                    return _result;
                }
                THandler handler = null;
                if (_segments.Length > 0)
                {
                    // check path root first
                    handler = RouteSubtree(_root, 0);
                    if (handler == null)
                    {
                        // check regex rules
                        for (var i = _regexRules.Count - 1; i >= 0; i--)
                        {
                            var ht = RouteRegex(_regexRules[i]);
                            if (ht != null)
                            {
                                handler = ht;
                                break;
                            }
                        }
                    }
                }
                _result = handler == null
                    ? new RoutingResult(false, new RoutingParameterCollection(), null)
                    : new RoutingResult(true,
                        new RoutingParameterCollection(_unnamedParams.ToArray(), _namedParams), handler);
                return _result;
            }

            private THandler RouteRegex(RegexRule rr)
            {
                Match m = rr.MatchExpression.Match(_fullPath);
                if (!m.Success)
                {
                    return null;
                }
                string[] groupNames = rr.GetMatchGroupNames();
                // copy last captured named group value
                foreach (var name in groupNames)
                {
                    _namedParams[name] = m.Groups[name].Value;
                }
                // copy all captures
                for (var i = 0; i < m.Groups.Count; i++)
                {
                    CaptureCollection caps = m.Groups[i].Captures;
                    for (var j = 0; j < caps.Count; j++)
                    {
                        _unnamedParams.Add(caps[j].Value);
                    }
                }
                return rr.Handler;
            }

            private THandler RouteSubtree(RoutingNode subRoot, int depth)
            {
                if (subRoot == null || depth >= _segments.Length)
                {
                    return null;
                }
                string curSegment = _segments[depth];
                if (depth == _segments.Length - 1)
                {
                    // reached final segment
                    switch (subRoot.Type)
                    {
                        case RoutingNodeType.Regular:
                            // normal matching
                            return subRoot.SegmentString.Equals(curSegment) ? subRoot.Handler : null;
                        case RoutingNodeType.NamedParameter:
                            // named parameter, matches ending slash
                            bool slashMatch = !(curSegment.EndsWith("/") ^
                                                subRoot.SegmentString.EndsWith("/"));
                            if (slashMatch)
                            {
                                _namedParams[subRoot.ParameterName] = curSegment.EndsWith("/")
                                    ? curSegment.Substring(0, curSegment.Length - 1)
                                    : curSegment;
                                return subRoot.Handler;
                            }
                            return null;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                // not yet last segment, but we encountered a leaf node, oops
                if (subRoot.IsLeaf)
                {
                    return null;
                }
                // not leaf node, let's go deeper
                var goDeeper = false;
                switch (subRoot.Type)
                {
                    case RoutingNodeType.Regular:
                        goDeeper = subRoot.SegmentString.Equals(curSegment);
                        break;
                    case RoutingNodeType.NamedParameter:
                        goDeeper = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (!goDeeper)
                {
                    return null;
                }
                // recursion, lasted added first
                for (var i = subRoot.Children.Count - 1; i >= 0; i--)
                {
                    RoutingNode node = subRoot.Children[i];
                    THandler handler = RouteSubtree(node, depth + 1);
                    if (handler != null)
                    {
                        // subtree matches, add possible named parameters
                        if (subRoot.Type == RoutingNodeType.NamedParameter)
                        {
                            _namedParams.Add(subRoot.ParameterName, curSegment.EndsWith("/")
                                ? curSegment.Substring(0, curSegment.Length - 1)
                                : curSegment);
                        }
                        return handler;
                    }
                }
                // nothing works
                return null;
            }

        }
    }
}
