using System;
using System.Collections.Generic;
using System.Linq;

namespace LauncherZLib.Plugin.Modules.Routing
{
    public static class UrlHelper
    {

        public static string[] GetSegmentsUnescaped(Uri url)
        {
            return GetFullPathAndSegmentsUnescaped(url).Item2;
        }

        public static string[] GetSegmentsUnescaped(string url)
        {
            return GetFullPathAndSegmentsUnescaped(url).Item2;
        }

        public static Tuple<string, string[]> GetFullPathAndSegmentsUnescaped(Uri url)
        {
            if (url.IsAbsoluteUri)
            {
                return new Tuple<string, string[]>(
                    Uri.UnescapeDataString(url.AbsolutePath),
                    url.Segments.Select(Uri.UnescapeDataString).ToArray());
            }
            return GetFullPathAndSegmentsUnescaped(url.OriginalString);
        }

        public static Tuple<string, string[]> GetFullPathAndSegmentsUnescaped(string url)
        {
            if (url == null)
                throw new ArgumentNullException("url");
            if (url.Length == 0)
            {
                // assume root for empty string
                return new Tuple<string, string[]>("/", new[] {"/"});
            }
            // assume escaped
            int schemeSepIdx = url.IndexOf("://", System.StringComparison.Ordinal);
            int querySepIdx = url.IndexOf('?');
            int fragmentSepIdx = url.IndexOf('#');
            if (fragmentSepIdx >= 0 && querySepIdx > fragmentSepIdx)
            {
                // "?" appears after "#"
                querySepIdx = -1;
            }
            if ((querySepIdx >= 0 && schemeSepIdx > querySepIdx) || (fragmentSepIdx >= 0 && schemeSepIdx > fragmentSepIdx))
            {
                // "://" appears after "?" or "#"
                schemeSepIdx = -1;
            }
            bool domainIncluded = schemeSepIdx >= 0;
            int startIdx = domainIncluded ? schemeSepIdx + 3 : (url[0] == '/' ? 1 : 0);
            int endIdx = querySepIdx < 0
                ? (fragmentSepIdx < 0 ? url.Length - 1 : fragmentSepIdx - 1)
                : (fragmentSepIdx < 0 ? querySepIdx - 1 : (Math.Min(querySepIdx, fragmentSepIdx) - 1));
            if (startIdx > endIdx)
            {
                // root
                return new Tuple<string, string[]>("/", new[] {"/"});
            }
            if (domainIncluded)
            {
                // skip domain part
                int nextSlashIdx = url.IndexOf('/', startIdx);
                if (nextSlashIdx < 0 || nextSlashIdx > endIdx)
                {
                    return new Tuple<string, string[]>("/", new[] { "/" });
                }
                if (nextSlashIdx > startIdx)
                {
                    startIdx = nextSlashIdx + 1;
                }
                else
                {
                    throw new FormatException(string.Format("Redundant slash at [{0}].", startIdx));
                }
            }
            string path = '/' + url.Substring(startIdx, endIdx - startIdx + 1);
            path = Uri.UnescapeDataString(path);
            // split
            var rawList = new List<string> { "/" };
            var lastIdx = 0;
            int curIdx;
            while ((curIdx = path.IndexOf('/', lastIdx + 1)) > -1)
            {
                var seg = path.Substring(lastIdx + 1, curIdx - lastIdx);
                if (seg.Length == 0)
                {
                    throw new FormatException("Url segment cannot be empty. Check if you have accidentally typed two consecutive slashes.");
                }
                rawList.Add(seg);
                lastIdx = curIdx;
            }
            if (lastIdx < path.Length - 1)
            {
                rawList.Add(path.Substring(lastIdx + 1));
            }
            return new Tuple<string, string[]>(path, rawList.ToArray());
        }
    }
}
