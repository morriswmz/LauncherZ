using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LauncherZ.Configuration
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LauncherZConfig
    {

        #region Private Fields

        private string _theme;
        private int _maxResultCount;

        #endregion

        #region Defaults

        public static string DefaultActivationKeyCombo
        {
            get { return "Win+OemQuestion"; }
        }

        public static string DefaultTheme
        {
            get { return "Default"; }
        }

        public static int DefaultMaxResultCount
        {
            get { return 100; }
        }

        #endregion

        public LauncherZConfig()
        {
            MaxResultCount = DefaultMaxResultCount;
            Priorities = new Dictionary<string, double>();
            ActivationKeyCombo = DefaultActivationKeyCombo;
            Theme = DefaultTheme;
        }

        [JsonProperty]
        public int MaxResultCount
        {
            get { return _maxResultCount; }
            set { _maxResultCount = Math.Max(1, value); }
        }

        [JsonProperty]
        public string ActivationKeyCombo { get; set; }

        [JsonProperty]
        public Dictionary<string, double> Priorities { get; private set; }

        [JsonProperty]
        public string Theme
        {
            get { return _theme; }
            set { _theme = value ?? DefaultTheme; }
        }
    }
}
