using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CorePlugins.BookmarkLauncher.Chrome;
using LauncherZLib.Event.Plugin;
using LauncherZLib.FormattedText;
using LauncherZLib.Launcher;
using LauncherZLib.Matching;
using LauncherZLib.Plugin;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Plugin.Template;

namespace CorePlugins.BookmarkLauncher
{
    [Plugin("LZBookmarkLauncher", Authors = "morriswmz", FriendlyName = "LauncherZ Bookmark Launcher", Version = "0.1.0.0")]
    [Description("Launches your browser favourites.")]
    public sealed class BookmarkLauncherPlugin : EmptyPlugin
    {
        private ConfigModule<BookmarkLauncherConfig> _configModule;
        private BookmarkLibrary _library; 

        public override void Activate(IPluginServiceProvider serviceProvider)
        {
            base.Activate(serviceProvider);
            _configModule = new ConfigModule<BookmarkLauncherConfig>(Logger, new BookmarkLauncherConfig());
            _configModule.ConfigFilePath = Path.Combine(PluginInfo.SuggestedPluginDataDirectory, "config.json");
            _configModule.LoadConfiguration();
            if (_configModule.Config.BookmarkSources == null || _configModule.Config.BookmarkSources.Length == 0)
            {
                _configModule.Config.BookmarkSources = GetDefaultSources();
            }
            // always initialize a new one here
            _library = new BookmarkLibrary(Path.Combine(PluginInfo.SuggestedPluginDataDirectory, "bookmarks.json"), Logger);
            _library.Readers.Add(new ChromeBookmarkReader());
            _library.Sources.AddRange(_configModule.Config.BookmarkSources);
            _library.LoadCachedBookmarks();
            if (DateTime.Now - _library.LastUpdateTime > new TimeSpan(0, 1, 0, 0))
            {
                _library.ScheduleUpdate();
            }
        }

        public override void Deactivate(IPluginServiceProvider serviceProvider)
        {
            // save cached bookmarks and clear references to readers/sources
            _library.SaveCachedBookmarks();
            _library.Sources.Clear();
            _library.Readers.Clear();
            _library = null;
            // save configurations
            _configModule.SaveConfiguration();
            _configModule = null;
            base.Deactivate(serviceProvider);
        }

        public override IEnumerable<LauncherData> Query(LauncherQuery query)
        {
            QueryTask(query);
            return base.Query(query);
        }

       

        public override PostLaunchAction Launch(LauncherData launcherData)
        {
            try
            {
                Process.Start(launcherData.StringData);
            }
            catch (Exception ex)
            {
                Logger.Warning(string.Format("Failed to open url \"{0}\". Details: {1}{2}",
                    launcherData.StringData, Environment.NewLine, ex));
            }
            return PostLaunchAction.Default;
        }

        private void QueryTask(LauncherQuery query)
        {
            var mathcer = new FlexMatcher();
            var scorer = new FlexScorer();
            var keywords = query.Arguments.ToArray();
#if DEBUG
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif
            IEnumerable<LauncherData> results = _library.Bookmarks.Select(b =>
            {
                FlexMatchResult nameMatch = mathcer.Match(b.Name, keywords, FlexLexicon.GlobalLexicon);
                FlexMatchResult urlMatch = mathcer.Match(b.Url, keywords, FlexLexicon.GlobalLexicon);
                if (!(nameMatch.Success || urlMatch.Success))
                    return null;
                double nameScore = scorer.Score(b.Name, nameMatch);
                double urlScore = scorer.Score(b.Url, urlMatch);
                double freqScore = (1.0 - Math.Exp(-b.Frequency/5.0));
                double finalScore = 0.5*nameScore + 0.3*urlScore + 0.2*freqScore;
                return finalScore > 0
                    ? new BookmarkQueryResult(
                        FormattedTextEngine.ConvertFlexMatchResult(b.Name, nameMatch),
                        FormattedTextEngine.ConvertFlexMatchResult(b.Url, urlMatch),
                        b.Url, finalScore)
                    : null;
            }).Where(x => x != null)
                .OrderByDescending(x => x.Relevance)
                .Take(10)
                .Select(x => x.ToLauncherData())
                .ToArray();
#if DEBUG
            sw.Stop();
            System.Diagnostics.Trace.WriteLine(string.Format("Bookmark Query Time: {0}ms", sw.ElapsedMilliseconds));
#endif
            if (EventBus != null)
            {
                EventBus.Post(new QueryResultUpdateEvent(query.QueryId, results));
            }
        }

        private BookmarkSource[] GetDefaultSources()
        {
            string chromePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Google\Chrome\User Data\Default\Bookmarks");
            return new BookmarkSource[]
            {
                new BookmarkSource("chrome", chromePath), 
            };
        }


    }
}
