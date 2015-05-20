namespace CorePlugins.BookmarkLauncher
{
    /// <summary>
    /// Represents an bookmark item.
    /// </summary>
    interface IBookmarkItem
    {
        /// <summary>
        /// Gets the name of the bookmark.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the URL of the bookmark.
        /// </summary>
        string Url { get; }
    }

}
