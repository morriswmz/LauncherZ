using Newtonsoft.Json;

namespace CorePlugins.BookmarkLauncher.Chrome
{
    [JsonObject]
    public class BookmarkItem : IBookmarkItem
    {
        [JsonProperty("children")]
        public BookmarkItem[] Children { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

    }
}
