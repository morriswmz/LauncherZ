using System.Collections.Generic;
using Newtonsoft.Json;

namespace CorePlugins.AppLauncher
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AppManifest
    {
        [JsonProperty(PropertyName = "apps", Required = Required.Always)]
        public List<AppDescription> Apps { get; set; }

        public static readonly AppManifest Empty = new AppManifest();

        public AppManifest()
        {
            Apps = new List<AppDescription>(0);
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class AppDescription
    {
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description", Required = Required.Always)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "link_file_location", Required = Required.Always)]
        public string LinkFileLocation { get; set; }

        [JsonProperty(PropertyName = "frequency", Required = Required.Always)]
        public int Frequency { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? "[Unnamed]" : Name;
        }
    }

}
