using Newtonsoft.Json;

namespace CorePlugins.BookmarkLauncher.Chrome
{
    [JsonObject]
    public class BookmarkRoot
    {
        [JsonProperty("bookmark_bar")]
        public BookmarkItem BookmarkBar { get; set; }
        
        [JsonProperty("other")]
        public BookmarkItem Other { get; set; }
    }
}
