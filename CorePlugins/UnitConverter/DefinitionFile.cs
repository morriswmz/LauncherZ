using System.ComponentModel;
using Newtonsoft.Json;

namespace CorePlugins.UnitConverter
{
    [JsonObject]
    public class DefinitionFile
    {
        [JsonProperty(Required = Required.Always)]
        public int Version { get; set; }

        [JsonProperty(Required = Required.Always)]
        public UnitDefinition[] Units { get; set; }

        [JsonProperty(Required = Required.Always)]
        public ConversionDefinition[] Conversions { get; set; }
    }

    [JsonObject]
    public class UnitDefinition
    {
        /// <summary>
        /// Full name of the unit.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// Optional abbreviation.
        /// Also counts as an alias.
        /// </summary>
        public string Abbreviation { get; set; }

        /// <summary>
        /// Possible aliases, separated by "|".
        /// Use "#" to specify unprefix unit name as an alias.
        /// e.g., if unit name is "volume.us.gal", then "#" represents "gal".
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Aliases { get; set; }
    }

    [JsonObject]
    public class ConversionDefinition
    {
        [JsonProperty(Required = Required.Always)]
        public string From { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string To { get; set; }

        [JsonProperty(Required = Required.Always)]
        public double Factor { get; set; }

        /// <summary>
        /// Optional offset (e.g. temperature conversion)
        /// Formula: To = Factor * From + Offset
        /// </summary>
        [DefaultValue(0.0)]
        public double Offset { get; set; }

    }

}
