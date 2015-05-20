using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LauncherZLib.Utils;
using Newtonsoft.Json;

namespace CorePlugins.BookmarkLauncher
{
    sealed class BookmarkLibrary
    {
        private readonly List<IBookmarkReader> _readers = new List<IBookmarkReader>();
        private readonly List<BookmarkSource> _sources = new List<BookmarkSource>();
        private Dictionary<string, CachedBookmark> _cachedBookmarkMap = new Dictionary<string, CachedBookmark>();
        private CachedBookmark[] _cachedBookmarks;
        private ILogger _logger;
        private DateTime _lastUpdateTime = DateTime.MinValue;
        private string _cachePath;

        private readonly object _taskLock = new object();
        private readonly object _dataLock = new object();
        

        public BookmarkLibrary(string cachePath, ILogger logger)
        {
            if (cachePath == null)
                throw new ArgumentNullException("cachePath");
            if (logger == null)
                throw new ArgumentNullException("logger");

            _cachePath = cachePath;
            _logger = logger;
        }

        /// <summary>
        /// Gets the list of bookmark sources.
        /// </summary>
        public List<BookmarkSource> Sources { get { return _sources; } }

        /// <summary>
        /// Gets the list of bookmark readers.
        /// </summary>
        public List<IBookmarkReader> Readers { get { return _readers; } }

        /// <summary>
        /// Gets the enumerator for bookmarks.
        /// </summary>
        public IEnumerable<IBookmarkItem> Bookmarks
        {
            get
            {
                lock (_dataLock)
                {
                    return _cachedBookmarks ?? _cachedBookmarkMap.Values.ToArray();
                }
            }
        }

        /// <summary>
        /// Gets or sets the path where cached bookmarks are stored.
        /// </summary>
        public string CacheFilePath
        {
            get { return _cachePath; }
            set { _cachePath = value ?? ""; }
        }

        /// <summary>
        /// Gets the last time of update.
        /// </summary>
        public DateTime LastUpdateTime
        {
            get { return _lastUpdateTime; }
        }

        /// <summary>
        /// Loads cached bookmarks from the file specified by cache path.
        /// </summary>
        public void LoadCachedBookmarks()
        {
            try
            {
                var list = JsonUtils.StreamDeserialize<CachedBookmarkList>(_cachePath);
                lock (_dataLock)
                {
                    _cachedBookmarkMap.Clear();
                    _cachedBookmarks = null;
                    if (list != null && list.Bookmarks != null)
                    {
                        foreach (var b in list.Bookmarks)
                        {
                            _cachedBookmarkMap[b.Url] = b;
                        }
                        _lastUpdateTime = new DateTime(list.Timestamp);
                    }
                    else
                    {
                        // no data saved, treat as never updated
                        _lastUpdateTime = DateTime.MinValue;
                    }
                }
                _logger.Fine(string.Format("Loaded {0} cached bookmarks.", _cachedBookmarkMap.Count));
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Failed to load cached bookmarks from \"{0}\". Details: {1}{2}", _cachePath, Environment.NewLine, ex));
                lock (_dataLock)
                {
                    _cachedBookmarkMap.Clear();
                    _cachedBookmarks = null;
                    // load fails, treat as never updated
                    _lastUpdateTime = DateTime.MinValue;
                }
            }
        }

        /// <summary>
        /// Saves cached bookmarks to the file specified by cache path.
        /// </summary>
        public void SaveCachedBookmarks()
        {
            CachedBookmarkList list;
            lock (_dataLock)
            {
                _cachedBookmarks = _cachedBookmarks ?? _cachedBookmarkMap.Values.ToArray();
                list = new CachedBookmarkList(_lastUpdateTime.Ticks, _cachedBookmarks);
            }
            try
            {
                JsonUtils.StreamSerialize(_cachePath, list, Formatting.Indented);
                _logger.Fine(string.Format("Saved {0} bookmarks.", list.Bookmarks.Length));
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Failed to save cached bookmarks to \"{0}\". Details: {1}{2}", _cachePath, Environment.NewLine, ex));
            }
        }

        /// <summary>
        /// Resets all usage frequencies to 0.
        /// </summary>
        public void ResetFrequencies()
        {
            lock (_dataLock)
            {
                foreach (var pair in _cachedBookmarkMap)
                {
                    pair.Value.Frequency = 0;
                }
            }
        }

        /// <summary>
        /// Schedules an update.
        /// </summary>
        public void ScheduleUpdate()
        {
            // copy readers and sources for the task
            var sources = _sources.ToArray();
            var readers = _readers.ToArray();
            Task.Run(() => ReadAndUpdateBookmarks(sources, readers));
        }

        /// <summary>
        /// Reads and update bookmarks.
        /// [Called in task]
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="readers"></param>
        private void ReadAndUpdateBookmarks(BookmarkSource[] sources, IBookmarkReader[] readers)
        {
            // only one task is allowed
            if (Monitor.TryEnter(_taskLock))
            {
                Dictionary<string, CachedBookmark> newBookmarkMap = ReadBookmarks(sources, readers);
                lock (_dataLock)
                {
                    // copy frequency data
                    foreach (var pair in _cachedBookmarkMap)
                    {
                        if (newBookmarkMap.ContainsKey(pair.Key))
                        {
                            newBookmarkMap[pair.Key].Frequency = pair.Value.Frequency;
                        }
                    }  
                    _cachedBookmarkMap = newBookmarkMap;
                    _cachedBookmarks = null;
                    _lastUpdateTime = DateTime.Now;
                }
                _logger.Fine(string.Format("Updated {0} bookmarks.", _cachedBookmarkMap.Count));
                Monitor.Exit(_taskLock);
            }
        }

        /// <summary>
        /// Actual implementation of bookmark reading process.
        /// [Called in task]
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="readers"></param>
        /// <returns></returns>
        private Dictionary<string, CachedBookmark> ReadBookmarks(BookmarkSource[] sources, IBookmarkReader[] readers)
        {
            // load new
            var bookmarkMap = new Dictionary<string, CachedBookmark>();
            foreach (var source in sources)
            {
                var success = false;
                foreach (var reader in readers)
                {
                    if (reader.SupportsBrowerType(source.BrowserType))
                    {
                        try
                        {
                            // if two bookmarks share the same url, only the first one is used
                            var results = reader.ReadBookmarks(source.Path)
                                .Where(b => !b.Url.StartsWith("javascript:", StringComparison.Ordinal) && !bookmarkMap.ContainsKey(b.Url))
                                .Select(b => new CachedBookmark(b.Name, b.Url));
                            foreach (var bookmark in results)
                            {
                                bookmarkMap[bookmark.Url] = bookmark;
                            }
                            success = true;
                            break;
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(string.Format("An exception occurred while reading bookmark. Details:{0}{1}",
                                Environment.NewLine, ex));
                        }   
                    }
                }
                if (!success)
                {
                    _logger.Warning(string.Format("Browser type \"{0}\" is not supported.", source.BrowserType));
                }
            }
            return bookmarkMap;
        }

    }
}
