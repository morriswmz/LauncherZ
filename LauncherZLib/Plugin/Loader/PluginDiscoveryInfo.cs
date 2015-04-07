using System;

namespace LauncherZLib.Plugin.Loader
{
    /// <summary>
    /// Contains information of a discovered plugin candidate.
    /// </summary>
    [Serializable]
    public class PluginDiscoveryInfo
    {
        private static readonly Version DefaultVersion = new Version(0, 0, 0, 0);

        public string Id { get; set; }
        public string FriendlyName { get; set; }
        public Version PluginVersion { get; set; }
        public string[] Authors { get; set; }
        public string Description { get; set; }

        public string Type { get; set; }
        public string AssemblyPath { get; set; }
        public string PluginClass { get; set; }
        public string[] Scripts { get; set; }

        public string SourceDirectory { get; set; }

        /// <summary>
        /// Gets the validity of discovered information.
        /// </summary>
        public bool IsValid { get; set; }
        /// <summary>
        /// If <see cref="P:LauncherZLib.Plugin.PluginDiscoveryInfo.IsValid"/> is false,
        /// this property contains the error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        public PluginDiscoveryInfo()
        {
            Id = "";
            FriendlyName = "";
            PluginVersion = DefaultVersion;
            Authors = new string[0];
            Description = "";

            Type = "Unknown";
            AssemblyPath = "";
            PluginClass = "";
            Scripts = new string[0];
            SourceDirectory = "";

            IsValid = false;
            ErrorMessage = "No valid information is set.";
        }

    }
}
