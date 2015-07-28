using System;
using System.IO;
using System.Reflection;

namespace LauncherZ.App
{
    sealed class LauncherZSpecialFolderManager
    {
        private bool _isPrepared = false;

        public static string PluginFolderName { get { return "Plugins"; } }
        public static string PluginDataFolderName { get { return "PluginData"; } }
        public static string LogFolderName { get { return "Logs"; } }
        public static string LexiconFolderName { get { return "Lexicons"; } }
        public static string ThemeFolderName { get { return "Themes"; } }
        public static string I18NFolderName { get { return "I18N"; } }

        /// <summary>
        /// Gets the full path of the folder containing the LauncherZ assembly.
        /// </summary>
        public string AssemblyFolder { get; private set; }

        /// <summary>
        /// Gets the full path of the default plugin folder, located in the assembly folder.
        /// </summary>
        public string DefaultPluginFolder { get; private set; }
        
        /// <summary>
        /// Gets the full path of the default lexicon folder, located in the assembly folder.
        /// </summary>
        public string DefaultLexiconFolder { get; private set; }

        /// <summary>
        /// Gets the full path of the default theme folder, located in the assembly folder.
        /// </summary>
        public string DefaultThemeFolder { get; private set; }

        /// <summary>
        /// Gets the full path of the application data folder (i.e., "%AppData%\LauncherZ.xxxxxx")
        /// </summary>
        public string UserDataFolder { get; private set; }

        /// <summary>
        /// Gets the full path of the folder containing log files.
        /// </summary>
        public string LogFolder { get; private set; }

        /// <summary>
        /// Gets the full path of the folder storing plugin data.
        /// </summary>
        public string PluginDataFolder { get; private set; }

        /// <summary>
        /// Gets the full path of the user plugin folder, located in the user data folder.
        /// </summary>
        public string UserPluginFolder { get; private set; }

        /// <summary>
        /// Gets the full path of the user lexicon folder, located in the user data folder.
        /// </summary>
        public string UserLexiconFolder { get; private set; }

        /// <summary>
        /// Gets the full path of the user theme folder, located in the user data folder.
        /// </summary>
        public string UserThemeFolder { get; private set; }

        /// <summary>
        /// Gets the full path of the localization file folder, located in the assembly folder.
        /// </summary>
        public string I18NFolder { get; private set; }
        
        /// <summary>
        /// Set up all special folders.
        /// </summary>
        public void PrepareFolders(LauncherZApp app)
        {
            if (_isPrepared)
                return;

            AssemblyFolder = Path.GetDirectoryName(Path.GetFullPath(Assembly.GetEntryAssembly().Location));
            if (AssemblyFolder == null)
                throw new Exception("Unable to determine the location of LauncherZ executable.");

            DefaultPluginFolder = Path.Combine(AssemblyFolder, PluginFolderName);
            DefaultLexiconFolder = Path.Combine(AssemblyFolder, LexiconFolderName);
            DefaultThemeFolder = Path.Combine(AssemblyFolder, ThemeFolderName);
            I18NFolder = Path.Combine(AssemblyFolder, I18NFolderName);

            string systemAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
#if DEBUG
            UserDataFolder = Path.Combine(systemAppDataPath, "LauncherZ.Debug." + app.AppGuid.Substring(0, 8));
#else
            UserDataFolder = Path.Combine(systemAppDataPath, "LauncherZ." + app.AppGuid.Substring(0, 8));
#endif
            CreateDirectionIfNotExist(UserDataFolder);

            LogFolder = Path.Combine(UserDataFolder, LogFolderName);
            CreateDirectionIfNotExist(LogFolder);

            PluginDataFolder = Path.Combine(UserDataFolder, PluginDataFolderName);
            CreateDirectionIfNotExist(PluginDataFolder);

            UserPluginFolder = Path.Combine(UserDataFolder, PluginFolderName);
            CreateDirectionIfNotExist(UserPluginFolder);

            UserLexiconFolder = Path.Combine(UserDataFolder, LexiconFolderName);
            CreateDirectionIfNotExist(UserLexiconFolder);

            UserThemeFolder = Path.Combine(UserDataFolder, ThemeFolderName);
            CreateDirectionIfNotExist(UserThemeFolder);
            
            _isPrepared = true;
        }

        private static void CreateDirectionIfNotExist(string dir)
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

    }
}
