using System;
using System.IO;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Utils;
using Newtonsoft.Json;

namespace LauncherZLib.Plugin.Template
{
    /// <summary>
    /// Describes a configurable plugin.
    /// </summary>
    /// <typeparam name="TC">Class of the configuration. Will be serialized/deserialized in JSON format.</typeparam>
    public abstract class ConfigurablePlugin<TC> : EmptyPlugin
        where TC : class
    {
        /// <summary>
        /// Plugin configuration.
        /// </summary>
        protected TC Configuration;
         
        /// <summary>
        /// Creates default configuration.
        /// </summary>
        /// <returns>Default configuration. Should never be null.</returns>
        protected abstract TC CreateDefaultConfiguration();

        public override void Activate(IPluginServiceProvider serviceProvider)
        {
            base.Activate(serviceProvider);
            LoadConfiguration();
        }

        public override void Deactivate(IPluginServiceProvider serviceProvider)
        {
            SaveConfiguration();
        }

        /// <summary>
        /// Load configuration from file.
        /// </summary>
        /// <remarks>
        /// Exceptions are logged.
        /// </remarks>
        protected virtual void LoadConfiguration()
        {
            var path = GetConfigurationFilePath();
            if (!File.Exists(path))
            {
                Configuration = CreateDefaultConfiguration();
                SaveConfiguration();
            }
            else
            {
                try
                {
                    Configuration = JsonUtils.StreamDeserialize<TC>(path);
                    Logger.Fine(string.Format(
                        "Successfully loaded configuration from: {0}.", path));
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format(
                        "An exception occurred while loading configuration." +
                        "File might be unaccessible or corrupted. Detailts:{0}{1}",
                        Environment.NewLine, ex));
                    Logger.Warning("Loading default configuration as fallback.");
                    Configuration = CreateDefaultConfiguration();
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
        /// Exceptions are logged.
        /// </remarks>
        protected virtual void SaveConfiguration()
        {
            var path = GetConfigurationFilePath();
            // check directory
            var dirPath = Path.GetDirectoryName(path);
            if (dirPath == null)
            {
                Logger.Error(string.Format("Invalid configuration file path: {0}.", path));
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
                    Logger.Error(string.Format(
                        "Failed to create directory for configuration file: {0}.", path));
                    return;
                }
            }
            // save
            try
            {
                JsonUtils.StreamSerialize(path, Configuration, Formatting.Indented);
                Logger.Fine(string.Format("Successfully saved configuration file: {0}.", path));
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format(
                    "An exception occurred while saving configuration file. Details: {0}{1}",
                    Environment.NewLine, ex));
            }
        }

        /// <summary>
        /// Retrieves the path of the configuration file.
        /// </summary>
        /// <returns>Full path of the configuration file. Should be consistent.</returns>
        protected virtual string GetConfigurationFilePath()
        {
            return Path.Combine(PluginInfo.SuggestedPluginDataDirectory, "config.json");
        }

    }
}
