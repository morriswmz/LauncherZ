using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using IWshRuntimeLibrary;
using LauncherZLib.FormattedText;
using LauncherZLib.Launcher;
using LauncherZLib.Matching;
using LauncherZLib.Utils;
using Newtonsoft.Json;
using File = System.IO.File;

namespace CorePlugins.AppLauncher
{
    public class AppManifestManager
    {
        private ILogger _logger;
        private AppManifest _manifest = AppManifest.Empty;
        private FlexMatcher _matcher = new FlexMatcher();
        private FlexScorer _scorer = new FlexScorer();
        private string _manifestPath;
        private bool _updating = false;
        private CancellationTokenSource _csSource;

        public AppManifestManager(string manifestPath, ILogger logger)
        {
            if (manifestPath == null)
                throw new ArgumentNullException("manifestPath");
            if (logger == null)
                throw new ArgumentNullException("logger");

            _manifestPath = manifestPath;
            _logger = logger;
        }

        public bool IsManifestEmpty { get { return _manifest.Apps == null || _manifest.Apps.Count == 0; } }

        public IEnumerable<AppDescription> GetAppDescriptions()
        {
            return IsManifestEmpty ? Enumerable.Empty<AppDescription>() : _manifest.Apps;
        } 

        public async void ScheduleUpdateManifest()
        {
            if (_updating)
                return;

            _updating = true;

            // start update task
            _csSource = new CancellationTokenSource();
            Dictionary<string, AppDescription> apps = await Task.Run(() => DoUpdate(), _csSource.Token);
            if (!_csSource.IsCancellationRequested)
            {
                // copy old data
                foreach (var oldApp in _manifest.Apps)
                {
                    AppDescription newApp;
                    if (apps.TryGetValue(oldApp.LinkFileLocation, out newApp))
                    {
                        newApp.Frequency = oldApp.Frequency;
                    }
                }
                // update existing
                _manifest.Apps = apps.Values.ToList();
                SaveManifestToFile();
            }
            _updating = false;
        }

        public void AbortUpdate()
        {
            if (_csSource != null && !_csSource.IsCancellationRequested)
                _csSource.Cancel();
        }

        public void IncreaseFrequencyFor(string lnkFilePath)
        {
            var app = _manifest.Apps.Find(
                x => x.LinkFileLocation.Equals(lnkFilePath, StringComparison.OrdinalIgnoreCase));
            if (app != null)
                app.Frequency++;
        }

        public void SaveManifestToFile()
        {
            if (!JsonUtils.TryStreamSerialize(_manifestPath, _manifest, Formatting.Indented))
                _logger.Error("Failed to save application manifest to {0}.", _manifestPath);
        }

        public bool LoadManifestFromFile()
        {
            AppManifest loadedManifest;
            if (!JsonUtils.TryStreamDeserialize(_manifestPath, out loadedManifest))
            {
                _logger.Error("Failed to load application manifest from {0}.", _manifestPath);
                return false;
            }
            _manifest = loadedManifest;
            return true;
        }

        public Dictionary<string, AppDescription> DoUpdate()
        {
            if (_csSource.IsCancellationRequested)
                return new Dictionary<string, AppDescription>(0);

            // init Windows Script Host
            IWshShell wsh = null;
            try
            {
                wsh = new WshShell();
            }
            catch (Exception)
            {
                return new Dictionary<string, AppDescription>(0);
            }
            // create directory walker
            var walker = new SafeDirectoryWalker
            {
                Recursive = true,
                MaxDepth = 8,
                SearchPattern = new Regex(@"\.lnk$", RegexOptions.IgnoreCase)
            };
            var apps = new Dictionary<string, AppDescription>();
            var aborted = false;
            // search for *.lnk
            var folders = new string[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                Environment.GetFolderPath(Environment.SpecialFolder.StartMenu)
            };
            foreach (var folder in folders)
            {
                walker.Walk(folder, fi =>
                {
                    if (_csSource.IsCancellationRequested)
                    {
                        aborted = true;
                        return false;
                    }

                    try
                    {
                        IWshShortcut shortcut = wsh.CreateShortcut(fi.FullName);
                        apps.Add(fi.FullName, new AppDescription()
                        {
                            Name = Path.GetFileNameWithoutExtension(fi.Name),
                            Description =
                                string.IsNullOrEmpty(shortcut.Description) ? fi.FullName : shortcut.Description,
                            LinkFileLocation = fi.FullName
                        });
                        return true;
                    }
                    catch (Exception)
                    {
                        System.Diagnostics.Trace.WriteLine(fi.FullName);
                        return true;
                    }
                });
            }


            return aborted ? new Dictionary<string, AppDescription>(0) : apps;
        }

    }
}
