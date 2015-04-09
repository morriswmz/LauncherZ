using System;
using System.Collections.Generic;
using System.IO;
using LauncherZLib.Plugin.Loader;

namespace LauncherZLib.Plugin.Service
{
    public class StaticPluginInfoProvider : IPluginInfoProvider
    {
        public string PluginId { get; private set; }

        public string PluginFriendlyName { get; private set; }

        public IEnumerable<string> PluginAuthors { get; private set; }

        public Version PluginVersion { get; private set; }

        public string PluginDescription { get; private set; }

        public string PluginSourceDirectory { get; private set; }

        public string SuggestedPluginDataDirectory { get; private set; }

        public StaticPluginInfoProvider(PluginDiscoveryInfo pdi, string dataDirBase)
        {
            PluginId = pdi.Id;
            PluginFriendlyName = pdi.FriendlyName;
            PluginAuthors = pdi.Authors;
            PluginVersion = pdi.PluginVersion;
            PluginDescription = pdi.Description;
            PluginSourceDirectory = pdi.SourceDirectory;
            SuggestedPluginDataDirectory = Path.Combine(dataDirBase, pdi.Id);
        }

    }
}
