using Newtonsoft.Json;

namespace CorePlugins.UnitConverter
{
    [JsonObject]
    public class DefinitionFile
    {
        [JsonProperty(Required = Required.Always)]
        public int Version { get; set; }

        [JsonProperty(Required = Required.Always)]
        public ConversionGroup[] ConversionGroups { get; set; }
    }

    [JsonObject]
    public class ConversionGroup
    {
        [JsonProperty(Required = Required.Always)]
        public string Dimension { get; set; }

        [JsonProperty(Required = Required.Always)]
        public UnitDefinition[] Units { get; set; }

        [JsonProperty(Required = Required.Always)]
        public ConversionDefinition[] Conversions { get; set; }
    }

    [JsonObject]
    public class UnitDefinition
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }

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

        protected bool Equals(ConversionDefinition other)
        {
            return string.Equals(From, other.From) && string.Equals(To, other.To) && Factor.Equals(other.Factor);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ConversionDefinition) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (From != null ? From.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (To != null ? To.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Factor.GetHashCode();
                return hashCode;
            }
        }
    }

}
