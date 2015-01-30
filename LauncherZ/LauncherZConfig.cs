using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LauncherZ
{
    [Serializable]
    public class LauncherZConfig
    {
        [JsonProperty("priorities")]
        public Dictionary<string, double> Priorities { get; private set; }
    }
}
