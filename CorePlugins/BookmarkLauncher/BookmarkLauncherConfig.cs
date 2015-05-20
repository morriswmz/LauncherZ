using Newtonsoft.Json;

namespace CorePlugins.BookmarkLauncher
{
    [JsonObject]
    sealed class BookmarkLauncherConfig
    {
        [JsonProperty]
        public BookmarkSource[] BookmarkSources { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    sealed class BookmarkSource
    {
        [JsonProperty]
        public string BrowserType { get; set; }

        [JsonProperty]
        public string Path { get; set; }

        public BookmarkSource()
        {
            
        }

        public BookmarkSource(string browserType, string path)
        {
            BrowserType = browserType;
            Path = path;
        }
    }
}
