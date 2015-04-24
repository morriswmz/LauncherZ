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

        public string ActivationKeyCombo { get; set; }

        public Dictionary<string, double> Priorities { get; private set; }

        public string LastLaunch { get; set; }


    }
}
