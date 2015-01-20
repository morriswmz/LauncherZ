using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CorePlugins.AppLauncher
{
    [Serializable]
    public class AppManifest
    {
        [JsonProperty(PropertyName = "apps", Required = Required.Always)]
        public List<AppDescription> Apps { get; set; }
    }

    [Serializable]
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


    }

}
