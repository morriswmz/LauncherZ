using System;
using System.IO;
using LauncherZLib.Utils;
using Newtonsoft.Json;

namespace LauncherZLib.Plugin.Modules
{
    /// <summary>
    /// Provides configuration via json.
    /// </summary>
    /// <typeparam name="TC">Configuration class.</typeparam>
    /// todo: add attributes for config class to control interative config module
    public class ConfigModule<TC> where TC : class
    {

        protected ILogger Logger;
        protected string FilePath;
        protected Func<TC> DefaultConfigConstructor;

        /// <summary>
        /// Initializes a configuration module with specified default configuration.
        /// </summary>
        /// <param name="logger">Logger instance. This value cannot be null.</param>
        /// <param name="defaultConfig">Default configuration. This value cannot be null.</param>
        public ConfigModule(ILogger logger, TC defaultConfig)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            if (defaultConfig == null)
                throw new ArgumentNullException("defaultConfig");
            Logger = logger;
            DefaultConfigConstructor = () => defaultConfig;
            Config = defaultConfig;
        }

        /// <summary>
        /// Initializes a configuration module with a default configuration constructor.
        /// </summary>
        /// <param name="logger">Logger instance. This value cannot be null.</param>
        /// <param name="defaultConfigConstructor">
        /// A function that returns the default configuration. The return value cannot be null.
        /// </param>
        public ConfigModule(ILogger logger, Func<TC> defaultConfigConstructor)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            if (defaultConfigConstructor == null)
                throw new ArgumentNullException("defaultConfigConstructor");
            Logger = logger;
            // wrap with null check
            DefaultConfigConstructor = () =>
            {
                TC dc = defaultConfigConstructor();
                if (dc == null)
                    throw new Exception("Default configuration constructor returns null.");
                return dc;
            };
            Config = defaultConfigConstructor();
        }

        /// <summary>
        /// Gets the configuration class.
        /// </summary>
        public TC Config { get; protected set; }

        /// <summary>
        /// Gets a copy of the default configuration.
        /// </summary>
        public TC DefaultConfig { get { return DefaultConfigConstructor(); } }

        /// <summary>
        /// Gets or sets the file path for loading/saving the configuration.
        /// </summary>
        /// <remarks>
        /// Configuration cannot be loaded/saved correctly if specified file path is invalid or unaccessible.
        /// </remarks>
        public string ConfigFilePath
        {
            get { return FilePath; }
            set {
                FilePath = value ?? "";
            }
        }

        /// <summary>
        /// Load configuration from file.
        /// </summary>
        /// <remarks>
        /// IO exceptions and deserialization errors are logged instead of thrown.
        /// Other unrecoverable exceptions will be thrown.
        /// </remarks>
        public virtual void LoadConfiguration()
        {
            if (!File.Exists(ConfigFilePath))
            {
                Config = DefaultConfigConstructor();
                SaveConfiguration();
            }
            else
            {
                try
                {
                    Config = JsonUtils.StreamDeserialize<TC>(ConfigFilePath);
                    Logger.Fine("Successfully loaded configuration from: {0}.", ConfigFilePath);
                }
                catch (Exception ex)
                {
                    Logger.Error(
                        "An exception occurred while loading configuration." +
                        "File might be unaccessible or corrupted. Detailts:{0}{1}",
                        Environment.NewLine, ex);
                    Logger.Warning("Loading default configuration as fallback.");
                    Config = DefaultConfigConstructor();
                    // possible corrupted file, replace with default
                    if (ex is JsonException)
                    {
                        Logger.Info("Attempting to overwrite existing configuration with default configuration");
                        SaveConfiguration();
                    }
                }
            }
        }

        /// <summary>
        /// Saves configuration to file.
        /// </summary>
        /// <remarks>
        /// IO exceptions and deserialization errors are logged instead of thrown.
        /// Other unrecoverable exceptions will be thrown.
        /// </remarks>
        public virtual void SaveConfiguration()
        {
            // check directory
            var dirPath = Path.GetDirectoryName(ConfigFilePath);
            if (dirPath == null)
            {
                Logger.Error("Invalid configuration file path: {0}.", ConfigFilePath);
                return;
            }
            if (!Directory.Exists(dirPath))
            {
                try
                {
                    Directory.CreateDirectory(dirPath);
                }
                catch (Exception)
                {
                    Logger.Error("Failed to create directory for configuration file: {0}.", ConfigFilePath);
                    return;
                }
            }
            // save
            try
            {
                JsonUtils.StreamSerialize(ConfigFilePath, Config, Formatting.Indented);
                Logger.Fine("Successfully saved configuration file: {0}.", ConfigFilePath);
            }
            catch (Exception ex)
            {
                Logger.Error("An exception occurred while saving configuration file. Details: {0}{1}",
                    Environment.NewLine, ex);
            }
        }

    }
}
