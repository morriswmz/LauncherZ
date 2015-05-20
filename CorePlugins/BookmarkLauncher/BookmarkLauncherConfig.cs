using Newtonsoft.Json;

namespace CorePlugins.BookmarkLauncher
{
    [JsonObject]
    class BookmarkLauncherConfig
    {
        [JsonProperty]
        public BookmarkSource[] BookmarkSources { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    class BookmarkSource
    {
        [JsonProperty]
        public string BrowserType { get; set; }

        [JsonProperty]
        public string Path { get; set; }
    }
}
