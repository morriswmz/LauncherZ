using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LauncherZLib.API;
using LauncherZLib.Event;
using LauncherZLib.Event.Launcher;
using LauncherZLib.FormattedText;
using LauncherZLib.Launcher;
using LauncherZLib.Matching;
using LauncherZLib.PluginTemplate;

namespace CorePlugins.AppLauncher
{
    public class AppLauncherPlugin : EmptyPlugin
    {

        private AppManifestManager _manager;
        private FlexMatcher _matcher = new FlexMatcher();
        private FlexScorer _scorer = new FlexScorer();
        private string _manifestFilePath;

        public override void Activate(IPluginContext pluginContext)
        {
            base.Activate(pluginContext);
            _manifestFilePath = Path.Combine(Context.SuggestedDataDirectory, "apps.json");
            _manager = new AppManifestManager(_manifestFilePath, Context.Logger);
            if (!_manager.LoadManifestFromFile() || _manager.IsManifestEmpty)
            {
                _manager.ScheduleUpdateManifest();
            }
            AddLexicons();
            Context.EventBus.Register(this);
        }

        public override void Deactivate(IPluginContext pluginContext)
        {
            _manager.AbortUpdate();
            _manager.SaveManifestToFile();
        }

        public override IEnumerable<LauncherData> Query(LauncherQuery query)
        {
            var candidates = new List<AppQueryResult>();
            foreach (var app in _manager.GetAppDescriptions())
            {
                FlexMatchResult matchResult = _matcher.Match(app.Name, query.Arguments.ToArray());
                if (matchResult.Success)
                {
                    double baseScore = _scorer.Score(app.Name, matchResult);
                    // weighted sum of match score and frequency of usage
                    // here we use exponential decay 1.0 - exp(-n)
                    double freqScore = 0.2 * (1.0 - Math.Exp(-app.Frequency / 5.0));
                    var result = new AppQueryResult(
                        FormattedTextEngine.ConvertFlexMatchResult(app.Name, matchResult),
                        app.Description, app.LinkFileLocation,
                        0.8 * baseScore + 0.2 * freqScore);
                    candidates.Add(result);
                }
            }
            return candidates.OrderByDescending(x => x.Relevance)
                .Take(10)
                .Select(x => x.CreateLauncherData())
                .ToArray();
        }

        [SubscribeEvent]
        public void LauncherExecutedHanlder(LauncherExecutedEvent e)
        {
            var prop = e.LauncherData.ExtendedProperties as AppLauncherExtendedProperties;
            if (prop != null)
            {
                try
                {
                    Process.Start(prop.LinkFileLocation);
                    _manager.IncreaseFrequencyFor(prop.LinkFileLocation);
                }
                catch (Exception)
                {
                    Context.Logger.Warning(string.Format("Unable to start process from: {0}", prop.LinkFileLocation));
                }
            }
        }

        private void AddLexicons()
        {
            string defaultLexiconPath = Path.Combine(Context.SourceDirectory, "Lexicons");
            if (Directory.Exists(defaultLexiconPath))
            {
                Context.Logger.Info(string.Format("Loading lexicons from default path: {0}.", defaultLexiconPath));
                AddLexiconsFromPath(defaultLexiconPath);
            }
            string userLexiconPath = Path.Combine(Context.SuggestedDataDirectory, "Lexicons");
            if (Directory.Exists(userLexiconPath))
            {
                Context.Logger.Info(string.Format("Loading lexicons from user data: {0}.", defaultLexiconPath));
                AddLexiconsFromPath(userLexiconPath);
            }
        }

        private void AddLexiconsFromPath(string path)
        {
            string[] lexicons;
            try
            {
                lexicons = Directory.GetFiles(path, "*.txt");
            }
            catch (Exception ex)
            {
                Context.Logger.Error(string.Format("Faild to get lexicon files from directory: {0}. Details:{1}{2}",
                    path, Environment.NewLine, ex));
                return;
            }
            foreach (var l in lexicons)
            {
                try
                {
                    _matcher.Lexicon.AddFromFile(l);
                }
                catch (Exception ex)
                {
                    Context.Logger.Error(string.Format(
                        "An exception occurred while loading lexicon: {0}. Details:{1}{2}",
                        l, Environment.NewLine, ex));
                }
            }
        }

    }
}
