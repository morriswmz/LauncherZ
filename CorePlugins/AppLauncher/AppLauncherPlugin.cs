using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LauncherZLib.Event;
using LauncherZLib.Event.Launcher;
using LauncherZLib.FormattedText;
using LauncherZLib.Launcher;
using LauncherZLib.Matching;
using LauncherZLib.Plugin;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Plugin.Template;
using LauncherZLib.Utils;

namespace CorePlugins.AppLauncher
{
    [Plugin("LZAppLauncher", FriendlyName = "LauncherZ Application Launcher", Authors = "morriswmz", Version = "0.1.0.0")]
    [Description("Launches installed applications.")]
    public class AppLauncherPlugin : EmptyPlugin
    {

        private AppManifestManager _manager;
        private FlexMatcher _matcher = new FlexMatcher();
        private FlexScorer _scorer = new FlexScorer();
        private ITimerService _timerService;
        private int _saveTimerId = -1;
        private string _manifestFilePath;

        public override void Activate(IExtendedServiceProvider serviceProvider)
        {
            base.Activate(serviceProvider);

            _timerService = serviceProvider.CanProvideService<ITimerService>()
                ? serviceProvider.GetService<ITimerService>()
                : null;
            if (_timerService != null)
            {
                _saveTimerId = _timerService.SetInterval(() => _manager.ScheduleUpdateManifest(), new TimeSpan(0, 0, 30, 0));
            }
            _manifestFilePath = Path.Combine(PluginInfo.SuggestedPluginDataDirectory, "apps.json");
            _manager = new AppManifestManager(_manifestFilePath, Logger);
            if (!_manager.LoadManifestFromFile() || _manager.IsManifestEmpty)
            {
                _manager.ScheduleUpdateManifest();
            }
            EventBus.Register(this);
        }

        public override void Deactivate(IExtendedServiceProvider serviceProvider)
        {
            if (_saveTimerId >= 0)
            {
                _timerService.ClearInterval(_saveTimerId);
                _saveTimerId = -1;
            }
            _manager.AbortUpdate();
            _manager.SaveManifestToFile();
            base.Deactivate(serviceProvider);
        }

        public override IEnumerable<LauncherData> Query(LauncherQuery query)
        {
            var candidates = new List<AppQueryResult>();
            foreach (var app in _manager.GetAppDescriptions())
            {
                FlexMatchResult matchResult = _matcher.Match(app.Name, query.Arguments.ToArray(), FlexLexicon.GlobalLexicon);
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
                .Select(x => x.ToLauncherData())
                .ToArray();
        }

        [SubscribeEvent]
        public void LauncherExecutedHanlder(LauncherExecutedEvent e)
        {
            try
            {
                Process.Start(e.LauncherData.StringData);
                _manager.IncreaseFrequencyFor(e.LauncherData.StringData);
            }
            catch (Exception)
            {
                Logger.Warning(string.Format("Unable to start process from: {0}", e.LauncherData.StringData));
            }
        }
    }
}
