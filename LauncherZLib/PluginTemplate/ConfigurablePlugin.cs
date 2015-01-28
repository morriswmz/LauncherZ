using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.API;
using LauncherZLib.Utils;
using Newtonsoft.Json;

namespace LauncherZLib.PluginTemplate
{
    /// <summary>
    /// Describes a configurable plugin.
    /// </summary>
    /// <typeparam name="T">Class of the configuration.</typeparam>
    public abstract class ConfigurablePlugin<T> : EmptyPlugin
        where T : class
    {
        /// <summary>
        /// Plugin configuration.
        /// </summary>
        protected T Configuration;
         
        /// <summary>
        /// Creates default configuration.
        /// </summary>
        /// <returns>Default configuration. Should never be null.</returns>
        protected abstract T CreateDefaultConfiguration();

        public override void Activate(IPluginContext pluginContext)
        {
            base.Activate(pluginContext);
            LoadConfiguration();
        }

        public override void Deactivate(IPluginContext pluginContext)
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
                    Configuration = JsonUtils.StreamDeserialize<T>(path);
                    Context.Logger.Fine(string.Format(
                        "Successfully loaded configuration from: {0}.", path));
                }
                catch (Exception ex)
                {
                    Context.Logger.Error(string.Format(
                        "An exception occurred while loading configuration." +
                        "File might be unaccessible or corrupted. Detailts:{0}{1}",
                        Environment.NewLine, ex));
                    Context.Logger.Warning("Loading default configuration as fallback.");
                    Configuration = CreateDefaultConfiguration();
                    // possible corrupted file, replace with default
                    if (ex is JsonException)
                    {
                        Context.Logger.Info("Attempting to overwrite existing configuration with default configuration");
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
                Context.Logger.Error(string.Format("Invalid configuration file path: {0}.", path));
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
                    Context.Logger.Error(string.Format(
                        "Failed to create directory for configuration file: {0}.", path));
                    return;
                }
            }
            // save
            try
            {
                JsonUtils.StreamSerialize(path, Configuration, Formatting.Indented);
                Context.Logger.Fine(string.Format("Successfully saved configuration file: {0}.", path));
            }
            catch (Exception ex)
            {
                Context.Logger.Error(string.Format(
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
            return Path.Combine(Context.SuggestedDataDirectory, "config.json");
        }

    }

}
