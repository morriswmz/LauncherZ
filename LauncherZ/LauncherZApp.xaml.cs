using System;
using System.Collections.Generic;
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
using LauncherZLib.Event.Plugin;
using LauncherZLib.Icon;
using LauncherZLib.Matching;
using LauncherZLib.Plugin;
using LauncherZLib.Utils;
using Newtonsoft.Json;

namespace LauncherZ
{
    /// <summary>
    /// Interaction logic for LauncherZApp.xaml
    /// </summary>
    public partial class LauncherZApp : Application
    {

        #region Private Fields

        private const string AppConfigFileName = "global_config.json";
        private bool _appInitialized = false;

        private FileIconProvider _fileIconProvider;
        private StaticIconProvider _staticIconProvider;

        #endregion

        #region Public Properties

        /// <summary>
        /// The current LaucherZApp instance.
        /// Same reference as Application.Current but no casting is required.
        /// </summary>
        public static LauncherZApp Instance { get; private set; }
        
        /// <summary>
        /// GUID of LauncherZ.
        /// </summary>
        public string AppGuid { get; private set; }

        public string LauncherZDataBasePath { get; private set; }
        public string PluginDataPath { get; private set; }
        public string LogPath { get; private set; }
        public string LexiconPath { get; private set; }

        #endregion

        #region Internal Properties

        internal LauncherZConfig Configuration { get; private set; }

        /// <summary>
        /// IconLibrary of LauncherZ.
        /// </summary>
        internal IconLibrary IconLibrary { get; private set; }

        internal SimpleLogger Logger { get; private set; }

        internal PluginManager PluginManager { get; private set; }

        internal IDispatcherService AppDispatcherService { get; private set; }

        #endregion

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
                // save configruation
                try
                {
                    JsonUtils.StreamSerialize(
                        Path.Combine(LauncherZDataBasePath, AppConfigFileName), Configuration, Formatting.Indented);
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format(
                        "An exception occurred while saving application configuration. Details:{0}{1}",
                        Environment.NewLine, ex));
                }
                // clean up
                Logger.Info("Deactivating plugins...");
                PluginManager.DeactivateAll();
                Logger.Fine("Exiting...");
                Logger.Close();
            }
        }

        private void InitilizeApp()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            // create app data folders
            CreateAppDataFolders();
            // initialize logger
            InitializeLogger();
            // load configurations
            try
            {
                Configuration = JsonUtils.StreamDeserialize<LauncherZConfig>(
                    Path.Combine(LauncherZDataBasePath, AppConfigFileName));
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format(
                    "An exception occurred while loading application configuration. Details: {0}{1}",
                    Environment.NewLine, ex));
                // default config
                Logger.Warning("Using default configuration.");
                Configuration = new LauncherZConfig();
            }
            // init icon library
            _staticIconProvider = new StaticIconProvider();
            _fileIconProvider = new FileIconProvider(Logger.CreateLogger("FileIconProvider"));
            var iconBorderBrush = FindResource("IconBorderBrush") as Brush;
            _fileIconProvider.ThumbnailBorderBrush = iconBorderBrush ?? Brushes.White;
            IconLibrary = new IconLibrary();
            IconLibrary.RegisterProvider(_staticIconProvider);
            IconLibrary.RegisterProvider(_fileIconProvider);
            RegitserInternalIcons();
            IconLibrary.DefaultIcon = _staticIconProvider.ProvideIcon(new IconLocation("LauncherZ", "IconBlank"));
            _fileIconProvider.MissingFileIcon = IconLibrary.DefaultIcon;
            // init and load plugins
            AppDispatcherService = new SimpleDispatcherService(Dispatcher);
            PluginManager = new PluginManager(Logger, AppDispatcherService);
            PluginManager.LoadAllFrom(Path.GetFullPath(@".\Plugins"), PluginDataPath);
            PluginManager.LoadAllFrom(string.Format("{0}{1}Plugins", LauncherZDataBasePath, Path.DirectorySeparatorChar), PluginDataPath);
            // set plugin priorities from configuration
            foreach (var pair in Configuration.Priorities)
            {
                PluginManager.SetPluginPriority(pair.Key, pair.Value);
            }
            // save priorities back to configuration
            Configuration.Priorities.Clear();
            foreach (var pluginId in PluginManager.LoadedPluginIds)
            {
                Configuration.Priorities.Add(pluginId, PluginManager.GetPluginPriority(pluginId));
            }
            // load global lexicons
            LoadLexiconsFrom(Path.GetFullPath(@".\Lexicons"));
            LoadLexiconsFrom(LexiconPath);
            // finish
            Logger.Fine("App started successfully.");
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

        private void CreateAppDataFolders()
        {
            string userAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            
#if DEBUG
            string launcherZDataBasePath = Path.Combine(userAppDataPath, "LauncherZ.Debug." + AppGuid.Substring(0,8));
#else
            string appDataBasePath = Path.Combine(userAppDataPath, "LauncherZ." + AppGuid.Substring(0, 8));
#endif
            if (!Directory.Exists(launcherZDataBasePath))
                Directory.CreateDirectory(launcherZDataBasePath);

            string pluginDataPath = Path.Combine(launcherZDataBasePath, "PluginData");
            if (!Directory.Exists(pluginDataPath))
                Directory.CreateDirectory(pluginDataPath);
            
            string logPath = Path.Combine(launcherZDataBasePath, "Logs");
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);
            
            string lexiconPath = Path.Combine(launcherZDataBasePath, "Lexicons");
            if (!Directory.Exists(lexiconPath))
                Directory.CreateDirectory(lexiconPath);

            LauncherZDataBasePath = launcherZDataBasePath;
            PluginDataPath = pluginDataPath;
            LogPath = logPath;
            LexiconPath = lexiconPath;
        }

        private void RegitserInternalIcons()
        {
            string[] internalIconName = { "IconProgram", "IconGear", "IconNetwork", "IconCalculator", "IconFolder", "IconBlank" };
            foreach (var s in internalIconName)
            {
                var bitmapImage = FindResource(s) as BitmapImage;
                _staticIconProvider.RegisterIcon(new IconLocation("LauncherZ", s), bitmapImage);
            }
        }

        private void LoadLexiconsFrom(string path)
        {
            
            if (!Directory.Exists(path))
                return;

            string[] lexicons;
            try
            {
                lexicons = Directory.GetFiles(path, "*.txt");
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("Faild to get lexicon files from directory \"{0}\". Details:{1}{2}",
                    path, Environment.NewLine, ex));
                return;
            }
            foreach (var l in lexicons)
            {
                try
                {
                    FlexLexicon.GlobalLexicon.AddFromFile(l);
                    Logger.Fine(string.Format("Loaded lexicon file \"{0}\".", l));
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format(
                        "An exception occurred while loading lexicon from \"{0}\". Details:{1}{2}",
                        l, Environment.NewLine, ex));
                }
            }
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
