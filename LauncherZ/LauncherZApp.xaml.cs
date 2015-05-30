using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LauncherZ.App;
using LauncherZ.Configuration;
using LauncherZ.Icon;
using LauncherZ.Windows;
using LauncherZLib;
using LauncherZLib.Event;
using LauncherZLib.I18N;
using LauncherZLib.Icon;
using LauncherZLib.Launcher;
using LauncherZLib.Matching;
using LauncherZLib.Plugin;
using LauncherZLib.Plugin.Loader;
using LauncherZLib.Plugin.Service;
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

        #endregion

        #region Internal Properties

        internal LauncherZSpecialFolderManager SpecialFolderManager { get; private set; }

        internal LauncherZConfig Configuration { get; private set; }

        internal EventBus GlobalEventBus { get; private set; }

        /// <summary>
        /// IconLibrary of LauncherZ.
        /// </summary>
        internal IconLibrary IconLibrary { get; private set; }

        internal SimpleLogger Logger { get; private set; }

        internal LaunchHistoryManager LaunchHistoryManager { get; private set; }

        internal PluginManager PluginManager { get; private set; }

        internal IDispatcherService AppDispatcherService { get; private set; }

        internal ITimerService AppTimerService { get; private set; }

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
                    Instance = this;
                }
                else
                {
                    Shutdown(1);
                    return;
                }
            }

            InitializeApp();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (_appInitialized)
            {
                // save configruation
                try
                {
                    JsonUtils.StreamSerialize(
                        Path.Combine(this.SpecialFolderManager.UserDataFolder, AppConfigFileName), Configuration, Formatting.Indented);
                }
                catch (Exception ex)
                {
                    Logger.Error(
                        "An exception occurred while saving application configuration. Details:{0}{1}",
                        Environment.NewLine, ex);
                }
                // clean up
                Logger.Info("Deactivating plugins...");
                PluginManager.DeactivateAll();
                Logger.Fine("Exiting...");
                Logger.Close();
            }
        }

        private void Application_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Severe(
                "An unhandled exception occurred. Application is unstable and terminating. Details: {0}{1}",
                Environment.NewLine, e.ExceptionObject);
            // emergency clean up
            if (_appInitialized)
            {
                Logger.Close();
            }
            // leave it unhandled
        }

        private void InitializeApp()
        {
            AppDomain.CurrentDomain.UnhandledException += Application_UnhandledException;
            // create app data folders
            SpecialFolderManager = new LauncherZSpecialFolderManager();
            SpecialFolderManager.PrepareFolders(this);
            // initialize logger
            InitializeLogger();
            // load configurations
            LoadConfigurations();
            // check theme
            SetUpTheme();
            // init icon library
            SetUpIconLibrary();
            // init basic services
            GlobalEventBus = new EventBus();
            AppDispatcherService = new SimpleDispatcherService(Dispatcher);
            AppTimerService = new SimpleTimer(Dispatcher);
            // load plugins
            LoadPlugins();
            // load global lexicons
            LoadLexiconsFrom(this.SpecialFolderManager.DefaultLexiconFolder);
            LoadLexiconsFrom(this.SpecialFolderManager.UserDataFolder);
            // start main window
            SetUpMainWindow();
            // finish
            Logger.Fine("App started successfully.");
            _appInitialized = true;
        }

        private void LoadPlugins()
        {
            var pluginLoader = new PluginLoader(
                new PluginDiscoverer(Logger.CreateLogger("PluginDiscoverer")),
                Logger.CreateLogger("PluginLoader"));
            var pcs = pluginLoader.LoadPlugins(new string[]
            {
                this.SpecialFolderManager.DefaultPluginFolder,
                this.SpecialFolderManager.UserPluginFolder
            }).Select(p =>
            {
                var essentialServices = new EssentialPluginServices(
                    new StaticPluginInfoProvider(p.Info, this.SpecialFolderManager.PluginDataFolder),
                    new LocalizationDictionary(), 
                    Logger.CreateLogger(p.Info.Id),
                    new PluginEventBus(p.Info.Id, GlobalEventBus, AppDispatcherService),
                    AppDispatcherService
                    );
                var psp = new PluginServiceProvider(essentialServices, new Dictionary<Type, object>()
                {
                    {typeof(ITimerService), AppTimerService},
                    {typeof(IIconRegisterer), _staticIconProvider},
                    {typeof(IIconProviderRegistry), IconLibrary}
                });
                return new PluginContainer(p.Instance, p.Info, psp);
            });
            PluginManager = new PluginManager(Logger);
            foreach (var pluginContainer in pcs)
            {
                PluginManager.AddAndActivate(pluginContainer);
            }
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
        }

        private void SetUpIconLibrary()
        {
            _staticIconProvider = new StaticIconProvider("static");
            _fileIconProvider = new FileIconProvider(Logger.CreateLogger("FileIconProvider"));
            var iconBorderBrush = FindResource("IconBorderBrush") as Brush;
            _fileIconProvider.ThumbnailBorderBrush = iconBorderBrush ?? Brushes.White;
            IconLibrary = new IconLibrary();
            IconLibrary.RegisterIconProvider(_staticIconProvider.Domain, _staticIconProvider);
            IconLibrary.RegisterIconProvider("file", _fileIconProvider);
            LauncherZIconSet.RegisterIconSet(this, _staticIconProvider);
            IconLibrary.DefaultIcon = _staticIconProvider.ProvideIcon(LauncherZIconSet.Blank);
            _fileIconProvider.MissingFileIcon = IconLibrary.DefaultIcon;
        }

        private void LoadConfigurations()
        {
            try
            {
                Configuration = JsonUtils.StreamDeserialize<LauncherZConfig>(
                    Path.Combine(this.SpecialFolderManager.UserDataFolder, AppConfigFileName));
            }
            catch (Exception ex)
            {
                Logger.Error("An exception occurred while loading application configuration. Details: {0}{1}",
                    Environment.NewLine, ex);
                // default config
                Logger.Warning("Using default configuration.");
                Configuration = new LauncherZConfig();
            }
        }

        private void SetUpTheme()
        {
            if (string.IsNullOrWhiteSpace(Configuration.Theme))
            {
                Configuration.Theme = LauncherZConfig.DefaultTheme;
            }
            if (!Configuration.Theme.Equals(LauncherZConfig.DefaultTheme, StringComparison.OrdinalIgnoreCase))
            {
                if (File.Exists(Configuration.Theme))
                {
                    try
                    {
                        var uri = new Uri(Path.GetFullPath(Configuration.Theme), UriKind.Absolute);
                        var rd = new ResourceDictionary() {Source = uri};
                        Resources.MergedDictionaries.Clear();
                        Resources.MergedDictionaries.Add(rd);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("An exception occurred while loading the theme file \"{0}\".{1}{2}",
                            Configuration.Theme, Environment.NewLine, ex);
                        var uri = new Uri("pack://application:,,,/Themes/DefaultTheme.xaml", UriKind.Absolute);
                        Resources.MergedDictionaries.Clear();
                        Resources.MergedDictionaries.Add(new ResourceDictionary() {Source = uri});
                        Configuration.Theme = LauncherZConfig.DefaultTheme;
                    }
                }
                else
                {
                    Logger.Warning("Theme file \"{0}\" does not exist.", Configuration.Theme);
                    Configuration.Theme = LauncherZConfig.DefaultTheme;
                }
            }
        }


        private void SetUpMainWindow()
        {
            var mw = new MainWindow(); // view model is initialized in MainWindow.xaml
            LaunchHistoryManager = new LaunchHistoryManager();
            var mainWindowController = new MainWindowController(this);
            mainWindowController.Attach(mw);
            mw.Show();
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
                Logger.Error("Faild to get lexicon files from directory \"{0}\". Details:{1}{2}",
                    path, Environment.NewLine, ex);
                return;
            }
            foreach (var l in lexicons)
            {
                try
                {
                    FlexLexicon.GlobalLexicon.AddFromFile(l);
                    Logger.Fine("Loaded lexicon file \"{0}\".", l);
                }
                catch (Exception ex)
                {
                    Logger.Error("An exception occurred while loading lexicon from \"{0}\". Details:{1}{2}",
                        l, Environment.NewLine, ex);
                }
            }
        }

        private void InitializeLogger()
        {
            // delete old logs
            new DirectoryInfo(this.SpecialFolderManager.LogFolder)
                .GetFiles("*.log")
                .OrderByDescending(f => f.LastWriteTime)
                .Skip(5)
                .ToList()
                .ForEach(f => f.Delete());
            // create new one
            string logFilePath = Path.Combine(this.SpecialFolderManager.LogFolder,
                string.Format("{0}.log", DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")));
            Logger = new SimpleLogger(logFilePath);
        }
        
        

    }
}
