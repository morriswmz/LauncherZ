using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LauncherZ
{
    [Serializable]
    public class LauncherZConfig
    {

        [JsonIgnore]
        public static string DefaultActivationKeyCombo
        {
            get { return "Win+OemQuestion"; }
        }



        public LauncherZConfig()
        {
            Priorities = new Dictionary<string, double>();
        }

        [JsonProperty("activation_key_combo")]
        public string ActivationKeyCombo { get; set; }

        [JsonProperty("priorities")]
        public Dictionary<string, double> Priorities { get; private set; }

    }
}
