using System.Collections.Generic;
using Newtonsoft.Json;

namespace LauncherZ.Configuration
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LauncherZConfig
    {

        #region Private Fields

        private string _theme;

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

        #endregion

        public LauncherZConfig()
        {
            Priorities = new Dictionary<string, double>();
            ActivationKeyCombo = DefaultActivationKeyCombo;
            Theme = DefaultTheme;
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
