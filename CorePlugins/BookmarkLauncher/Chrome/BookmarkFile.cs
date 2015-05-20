using Newtonsoft.Json;

namespace CorePlugins.BookmarkLauncher.Chrome
{
    [JsonObject]
    sealed public class BookmarkFile
    {
        [JsonProperty("checksum")]
        public string Checksum { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        [JsonProperty("roots")]
        public BookmarkRoot Roots { get; set; }
    }
}
