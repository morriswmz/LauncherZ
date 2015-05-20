using System;
using System.Collections.Generic;
using LauncherZLib.Utils;

namespace CorePlugins.BookmarkLauncher.Chrome
{
    class ChromeBookmarkReader : IBookmarkReader
    {
        public bool SupportsBrowerType(string type)
        {
            return type != null && type.Trim().Equals("chrome", StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<IBookmarkItem> ReadBookmarks(string path)
        {
            var bookmarkFile = JsonUtils.StreamDeserialize<BookmarkFile>(path);
            var bookmarks = new List<BookmarkItem>();
            var foldersToVisit = new Stack<BookmarkItem>();
            foldersToVisit.Push(bookmarkFile.Roots.BookmarkBar);
            foldersToVisit.Push(bookmarkFile.Roots.Other);
            while (foldersToVisit.Count > 0)
            {
                BookmarkItem current = foldersToVisit.Pop();
                if (current.Type == "folder")
                {
                    foreach (var b in current.Children)
                    {
                        foldersToVisit.Push(b);
                    }
                }
                else if (current.Type == "url")
                {
                    bookmarks.Add(current);
                }
            }
            return bookmarks;
        }
    }
}
