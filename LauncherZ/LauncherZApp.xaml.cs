using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LauncherZLib.Icon;
using LauncherZLib.Plugin;
using LauncherZLib.Utils;

namespace LauncherZ
{
    /// <summary>
    /// Interaction logic for LauncherZApp.xaml
    /// </summary>
    public partial class LauncherZApp : Application
    {

        #region Private Fields

        private bool _appInitialized = false;

        #endregion

        /// <summary>
        /// The current LaucherZApp instance.
        /// Same reference as Application.Current but no casting is required.
        /// </summary>
        public static LauncherZApp Instance { get; private set; }

        /// <summary>
        /// IconManager of LauncherZ.
        /// </summary>
        public IconManager IconManager { get; private set; }

        public SimpleLogger Logger { get; private set; }

        internal PluginManager PluginManager { get; private set; }
        
        /// <summary>
        /// GUID of LauncherZ.
        /// </summary>
        public string AppGuid { get; private set; }

        public string AppDataBasePath { get; private set; }
        public string PluginDataPath { get; private set; }
        public string LogPath { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // not very necessary to check this
            if (Instance != null)
                throw new Exception("Only one instance of LaucherZApp can be created.");

            // read GUID
            Assembly assembly = Assembly.GetEntryAssembly();
            var guidAttr = assembly.GetCustomAttribute(typeof(GuidAttribute)) as GuidAttribute;
            if (guidAttr == null || string.IsNullOrEmpty(guidAttr.Value))
            {
                // this should never happen
                Shutdown(1);
                return;
            }

            // single instance check
            bool createNew = true;
            var mutexId = "LauncherZ:" + guidAttr.Value;
            using (var mutex = new Mutex(true, mutexId, out createNew))
            {
                if (createNew)
                {
                    AppGuid = guidAttr.Value;
                    StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
                    Instance = this;
                }
                else
                {
                    Shutdown(1);
                    return;
                }
            }

            InitilizeApp();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (_appInitialized)
            {
                // clean up
                Logger.Close();
            }
        }

        private void InitilizeApp()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            // create app data files
            CreateAppDataFiles();
            // initialize logger
            InitializeLogger();
            // load configurations
            Logger.Info("Initialized.");
            // initialize components
            PluginManager = new PluginManager(Logger);
            IconManager = new IconManager(512, PluginManager);
            RegitserInternalIcons();
            // load plugins
            PluginManager.LoadAllFrom(Path.GetFullPath(@".\Plugins"), PluginDataPath);
            PluginManager.LoadAllFrom(string.Format("{0}{1}Plugins", AppDataBasePath, Path.DirectorySeparatorChar), PluginDataPath);

            _appInitialized = true;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Severe(string.Format(
                "An unhandled exception occurred. Application is unstable and terminating. Details: {0}{1}",
                Environment.NewLine, e.ExceptionObject));
            // emergency clean up
            if (_appInitialized)
            {
                Logger.Close();
            }
            // leave it unhandled
        }

        private void CreateAppDataFiles()
        {
            string userAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            
#if DEBUG
            string appDataBasePath = string.Format("{0}\\LauncherZ.Debug.{1}", userAppDataPath, AppGuid.Substring(0,8));
#else
            string appDataBasePath = string.Format("{0}\\LauncherZ.{1}", userAppDataPath, AppGuid.Substring(0, 8));
#endif
            if (!Directory.Exists(appDataBasePath))
                Directory.CreateDirectory(appDataBasePath);
            string pluginDataPath = appDataBasePath + "\\PluginData";
            if (!Directory.Exists(pluginDataPath))
                Directory.CreateDirectory(pluginDataPath);
            string logPath = appDataBasePath + "\\Logs";
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            AppDataBasePath = appDataBasePath;
            PluginDataPath = pluginDataPath;
            LogPath = logPath;
        }

        private void RegitserInternalIcons()
        {
            string[] internalIconName = { "IconProgram", "IconGear", "IconNetwork", "IconCalculator", "IconFolder", "IconBlank" };
            foreach (var s in internalIconName)
            {
                var bitmapImage = FindResource(s) as BitmapImage;
                IconManager.AddIcon(new IconLocation("LauncherZ", s), bitmapImage, true);
            }
            IconManager.DefaultIcon = IconManager.GetIcon(new IconLocation("LauncherZ", "IconBlank"));
            IconManager.ThumbnailBorderBrush = Brushes.White;
        }

        private void InitializeLogger()
        {
            // delete old logs
            new DirectoryInfo(LogPath)
                .GetFiles("*.log")
                .OrderByDescending(f => f.LastWriteTime)
                .Skip(5)
                .ToList()
                .ForEach(f => f.Delete());
            // create new one
            Logger = new SimpleLogger(string.Format("{0}\\{1}.log", LogPath, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")));
        }
        
        

    }
}
