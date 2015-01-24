using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.API;
using LauncherZLib.FormattedText;
using LauncherZLib.Launcher;
using LauncherZLib.Matching;
using Newtonsoft.Json;

namespace CorePlugins.AppLauncher
{
    public class AppManifestManager
    {
        private ILogger _logger;
        private AppManifest _manifest;
        private FlexMatcher _matcher = new FlexMatcher();
        private FlexScorer _scorer = new FlexScorer();

        public AppManifestManager(ILogger logger)
        {
            _logger = logger;
        }


        public IEnumerable<LauncherData> Query(string[] keywords, int limit)
        {
            var candidates = new List<AppQueryResult>();
            foreach (var app in _manifest.Apps)
            {
                FlexMatchResult matchResult = _matcher.Match(app.Name, keywords);
                if (matchResult.Success)
                {
                    double baseScore = _scorer.Score(app.Name, matchResult);
                    // weighted sum of match score and frequency of usage
                    // here we use exponential decay 1.0 - exp(-n)
                    double freqScore = 0.2*(1.0 - Math.Exp(-app.Frequency/5.0));
                    var result = new AppQueryResult(
                        FormattedTextEngine.ConvertFlexMatchResult(app.Name, matchResult),
                        app.Description, app.LinkFileLocation,
                        0.8*baseScore + 0.2*freqScore);
                    candidates.Add(result);
                }
            }
            return candidates.OrderByDescending(x => x.Relevance)
                .Take(limit)
                .Select(x => x.CreateLauncherData());
        }

        public void ScheduleUpdateManifest()
        {
            
        }

        public void SaveManifestToFile(string path)
        {
            try
            {
                var serializer = new JsonSerializer();
                using (var sw = new StreamWriter(path))
                {
                    serializer.Serialize(sw, _manifest);
                    sw.Flush();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Failed to save application manifest to {0}", path));
            }
        }

        public bool LoadManifestFromFile(string path)
        {
            if (!File.Exists(path))
                return false;

            try
            {
                var serializer = new JsonSerializer();
                using (var sr = new StreamReader(path))
                {
                    var jsonReader = new JsonTextReader(sr);
                    _manifest = serializer.Deserialize<AppManifest>(jsonReader);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Failed to load application manifest from {0}", path));
                return false;
            }
        }

    }
}
