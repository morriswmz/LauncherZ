using System.Collections.Generic;
using Newtonsoft.Json;

namespace LauncherZLib.LauncherTask.Provider
{
    public class TaskProviderManifest
    {
        [JsonProperty(PropertyName = "providers")]
        public List<TaskProviderInfo> Providers { get; set; }
    }

    public class TaskProviderInfo
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

        [JsonProperty(PropertyName = "provider_type", Required = Required.Always)]
        public string ProviderType { get; set; }

        [JsonProperty(PropertyName = "assembly")]
        public string Assembly { get; set; }

        [JsonProperty(PropertyName = "provider_class")]
        public string ProviderClass { get; set; }

        [JsonProperty(PropertyName = "xml")]
        public string Xml { get; set; }

        [JsonProperty(PropertyName = "command_line")]
        public string CommandLine { get; set; }

        [JsonIgnore]
        public string SourceDirectory { get; set; }

        [JsonIgnore]
        public string DataDirectory { get; set; }

        public TaskProviderInfo()
        {
            Description = string.Empty;
            Priority = 0.0;
            Assembly = string.Empty;
            ProviderClass = string.Empty;
            Xml = string.Empty;
            CommandLine = string.Empty;
        }

    }

    public static class TaskProviderType
    {
        public static readonly string Assembly = "assembly";
        public static readonly string Xml = "xml";
        public static readonly string Command = "command";
    }
}
