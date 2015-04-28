using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LauncherZ
{
    [Serializable]
    public class LauncherZConfig
    {

        #region Private Fields

        private int _maxLaunchHistory;

        #endregion

        [JsonIgnore]
        public static string DefaultActivationKeyCombo
        {
            get { return "Win+OemQuestion"; }
        }

        public LauncherZConfig()
        {
            Priorities = new Dictionary<string, double>();
            ActivationKeyCombo = DefaultActivationKeyCombo;
            MaxLaunchHistory = 10;
            LaunchHistory = new string[0];
        }

        public string ActivationKeyCombo { get; set; }

        public Dictionary<string, double> Priorities { get; private set; }

        public int MaxLaunchHistory
        {
            get { return _maxLaunchHistory; }
            set
            {
                if (!double.IsNaN(value))
                    _maxLaunchHistory = Math.Min(Math.Max(value, 1), 1000);
            }
        }

        public string[] LaunchHistory { get; set; }

    }
}
