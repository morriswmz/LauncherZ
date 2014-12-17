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
using System.Windows.Media.Imaging;
using LauncherZLib.Icon;
using LauncherZLib.LauncherTask.Provider;
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

        internal TaskProviderManager ProviderManager { get; private set; }
        
        /// <summary>
        /// GUID of LauncherZ.
        /// </summary>
        public string AppGuid { get; private set; }

        public string AppDataBasePath { get; private set; }
        public string ProviderDataPath { get; private set; }
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
            // create app data files
            CreateAppDataFiles();
            // initialize logger
            InitializeLogger();
            // load configurations
            Logger.Info("Initialized.");
            // initialize components
            ProviderManager = new TaskProviderManager();
            IconManager = new IconManager(512, ProviderManager);
            RegisterInternalIcon("IconProgram");
            RegisterInternalIcon("IconGear");
            RegisterInternalIcon("IconNetwork");
            RegisterInternalIcon("IconCalculator");
            IconManager.DefaultIcon = IconManager.GetIcon(new IconLocation("LauncherZ", "IconProgram"));

            // load plugins
            ProviderManager.LoadAllFrom(Path.GetFullPath(@".\Providers"), ProviderDataPath, Logger);

            _appInitialized = true;
        }

        private void CreateAppDataFiles()
        {
            string userAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            
#if DEBUG
            string appDataBasePath = string.Format("{0}\\LauncherZ.Debug.{1}", userAppDataPath, AppGuid.Substring(0,8));
#else
            string appDataBasePath = string.Format("{0}\\LauncherZ.{1}", appDataPath, AppGuid.Substring(0,8));
#endif
            if (!Directory.Exists(appDataBasePath))
                Directory.CreateDirectory(appDataBasePath);
            string providerDataPath = appDataBasePath + "\\Providers";
            if (!Directory.Exists(providerDataPath))
                Directory.CreateDirectory(providerDataPath);
            string logPath = appDataBasePath + "\\Logs";
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            AppDataBasePath = appDataBasePath;
            ProviderDataPath = providerDataPath;
            LogPath = logPath;
        }

        private void RegisterInternalIcon(string name)
        {
            var bitmapImage = FindResource(name) as BitmapImage;
            IconManager.RegisterPersistentIcon(new IconLocation("LauncherZ", name), bitmapImage);
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
            Logger = new SimpleLogger(string.Format("{0}\\{1}.log", LogPath, DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss")));
        }
        
        

    }
}
