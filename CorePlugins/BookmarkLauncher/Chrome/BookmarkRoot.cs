using Newtonsoft.Json;

namespace CorePlugins.BookmarkLauncher.Chrome
{
    [JsonObject]
    sealed public class BookmarkRoot
    {
        [JsonProperty("bookmark_bar")]
        public BookmarkItem BookmarkBar { get; set; }
        
        [JsonProperty("other")]
        public BookmarkItem Other { get; set; }
    }
}
