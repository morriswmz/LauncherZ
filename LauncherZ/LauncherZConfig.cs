using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LauncherZ
{
    [Serializable]
    public class LauncherZConfig
    {

        public LauncherZConfig()
        {
            Priorities = new Dictionary<string, double>();
        }

        [JsonProperty("priorities")]
        public Dictionary<string, double> Priorities { get; private set; }

        
    }
}
