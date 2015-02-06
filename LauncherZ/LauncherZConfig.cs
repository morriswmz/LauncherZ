using System;
using System.Collections.Generic;
using System.Windows.Input;
using Newtonsoft.Json;

namespace LauncherZ
{
    [Serializable]
    public class LauncherZConfig
    {

        public static string DefaultActivationKeyCombo
        {
            get { return "Win+OemQuestion"; }
        }



        public LauncherZConfig()
        {
            Priorities = new Dictionary<string, double>();
        }

        [JsonProperty("priorities")]
        public Dictionary<string, double> Priorities { get; private set; }

    }
}
