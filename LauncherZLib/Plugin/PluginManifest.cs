using System.Collections.Generic;
using Newtonsoft.Json;

namespace LauncherZLib.Plugin
{
    public class PluginManifest
    {
        [JsonProperty(PropertyName = "plugins")]
        public List<PluginInfo> Plugins { get; set; }
    }

    public class PluginInfo
    {

        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "authors", Required = Required.Always)]
        public List<string> Authors { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "version", Required = Required.Always)]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "priority")]
        public double Priority { get; set; }

        [JsonProperty(PropertyName = "plugin_type", Required = Required.Always)]
        public string PluginType { get; set; }

        [JsonProperty(PropertyName = "assembly")]
        public string Assembly { get; set; }

        [JsonProperty(PropertyName = "plugin_class")]
        public string PluginClass { get; set; }

        [JsonProperty(PropertyName = "xml")]
        public string Xml { get; set; }

        [JsonProperty(PropertyName = "command_line")]
        public string CommandLine { get; set; }

        [JsonIgnore]
        public string SourceDirectory { get; set; }

        [JsonIgnore]
        public string DataDirectory { get; set; }

        public PluginInfo()
        {
            Description = string.Empty;
            Priority = 0.0;
            Assembly = string.Empty;
            PluginClass = string.Empty;
            Xml = string.Empty;
            CommandLine = string.Empty;
        }

    }

    public static class PluginType
    {
        public static readonly string Assembly = "assembly";
        public static readonly string Xml = "xml";
        public static readonly string Command = "command";
    }
}
