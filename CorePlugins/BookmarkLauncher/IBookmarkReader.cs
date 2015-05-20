using System.Collections.Generic;

namespace CorePlugins.BookmarkLauncher
{
    interface IBookmarkReader
    {
        bool SupportsBrowerType(string type);

        IEnumerable<IBookmarkItem> ReadBookmarks(string path);
    }
}
