using System.Collections.Generic;
using Newtonsoft.Json;

namespace CorePlugins.BookmarkLauncher
{
    [JsonObject]
    sealed class CachedBookmarkList
    {
        public long Timestamp { get; set; }
        public CachedBookmark[] Bookmarks { get; set; }

        public CachedBookmarkList()
        {
        }

        public CachedBookmarkList(long timestamp, CachedBookmark[] bookmarks)
        {
            Timestamp = timestamp;
            Bookmarks = bookmarks;
        }
    }

    [JsonObject]
    sealed class CachedBookmark : IBookmarkItem
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public int Frequency { get; set; }

        public CachedBookmark()
        {
        }

        public CachedBookmark(string name, string url)
        {
            Name = name;
            Url = url;
        }


    }
}
