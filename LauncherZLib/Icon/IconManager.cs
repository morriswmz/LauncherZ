using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LauncherZLib.API;
using LauncherZLib.Utils;

namespace LauncherZLib.Icon
{
    [Obsolete]
    public class IconManager
    {

        public delegate void IconLoadedHandler(IconLocation location, BitmapSource icon);

        // stores persistent icons
        private Dictionary<IconLocation, BitmapSource> _persistentIcons = new Dictionary<IconLocation, BitmapSource>();
        // stores cached icons, provides quick lookup of non-persistent icons
        private SimpleCache<IconLocation, BitmapSource> _cachedIcons;
        // caches loaded icons
        private SimpleCache<string, BitmapSource> _loadingCache;
 
        private ConcurrentQueue<IconLocation> _pending = new ConcurrentQueue<IconLocation>();
        private Dictionary<IconLocation, List<IconLoadedHandler>> _handlers = new Dictionary<IconLocation, List<IconLoadedHandler>>();
        private IIconLocationResolver _resolver;
        private readonly object _loadLock = new object();

        private static readonly string DirectoryKey = ".";

        public IconManager(int cacheCapacity, IIconLocationResolver resolver)
        {
            _resolver = resolver;
            _cachedIcons = new SimpleCache<IconLocation, BitmapSource>(cacheCapacity);
            _loadingCache = new SimpleCache<string, BitmapSource>(cacheCapacity);
        }

        public Brush ThumbnailBorderBrush { get; set; }

        public BitmapSource DefaultIcon { get; set; }

        public bool ContainsIcon(IconLocation location)
        {
            lock (_loadLock)
            {
                return _persistentIcons.ContainsKey(location) || _cachedIcons.ContainsKey(location);
            }
        }

        public BitmapSource GetIcon(IconLocation location)
        {
            lock (_loadLock)
            {
                BitmapSource icon;
                if (_persistentIcons.TryGetValue(location, out icon))
                    return icon;
                if (_cachedIcons.TryGet(location, out icon))
                    return icon;
                return DefaultIcon;
            }
        }
        
        public void AddIcon(IconLocation location, bool persistent)
        {
            throw new NotImplementedException();
        }

        public void AddIcon(IconLocation location, BitmapSource icon, bool persistent)
        {
            if (!icon.IsFrozen)
            {
                icon.Freeze();
            }
            lock (_loadLock)
            {
                if (persistent)
                {
                    _persistentIcons[location] = icon;
                }
                else
                {
                    _cachedIcons[location] = icon;
                }
                
            }
        }

        public async void AddIconAsync(IconLocation location, bool persistent, IconLoadedHandler callback)
        {
            bool initLoad = false;
            lock (_loadLock)
            {
                BitmapSource icon;
                // check if icon is already loaded
                if (_persistentIcons.TryGetValue(location, out icon))
                {
                    callback(location, icon);
                    return;
                }
                if (_cachedIcons.TryGet(location, out icon))
                {
                    callback(location, icon);
                    return;
                }
                // check if load is initialized
                if (_handlers.ContainsKey(location))
                {
                    _handlers[location].Add(callback);
                }
                else
                {
                    _handlers[location] = new List<IconLoadedHandler> {callback};
                    // initialize load
                    initLoad = true;
                }
            }
            if (initLoad)
            {
                BitmapSource icon = await Task.Run(() => AddImpl(location)) ?? DefaultIcon;
                lock (_loadLock)
                {
                    if (persistent)
                        _persistentIcons[location] = icon;
                    else
                        _cachedIcons[location] = icon;
                    foreach (var handler in _handlers[location])
                        handler(location, icon);
                    _handlers[location].Clear();
                    _handlers.Remove(location);
                }
            }
        }

        private BitmapSource AddImpl(IconLocation location)
        {
            string path;
            if (!_resolver.TryResolve(location, out path))
                return null;
            // check file
            BitmapSource icon = null;
            if (Directory.Exists(path))
            {
                // check directory icon
                if (_loadingCache.TryGet(DirectoryKey, out icon))
                    return icon;
                icon = IconLoader.LoadDirectoryIcon(IconSize.Large);
                if (icon != null)
                    _loadingCache[DirectoryKey] = icon;
            }
            else if (File.Exists(path))
            {
                // check normal file icon
                path = Path.GetFullPath(path);
                // check loading cache first
                if (_loadingCache.TryGet(path, out icon))
                    return icon;
                string ext = Path.GetExtension(path);
                // supported image file, generate thumbnail
                if (IconLoader.IsSupportedImageFileExtension(ext))
                {
                    // generate thumbnail first
                    icon = IconLoader.LoadThumbnail(path, IconSize.Large, ThumbnailBorderBrush);
                    if (icon != null)
                    {
                        _loadingCache[path] = icon;
                        return icon;
                    }
                    // if fails, continue execution and use file type icon (out of if)
                }
                // check loading cache for extension
                if (!string.IsNullOrEmpty(ext) && _loadingCache.TryGet(ext, out icon))
                    return icon;
                // load asssociated icon
                icon = IconLoader.LoadFileIcon(path, IconSize.Large);
                // save general file type icon if successful
                // note that .exe and .lnk have distinct icons and are skipped
                if (icon != null)
                {
                    if (!string.IsNullOrEmpty(ext) &&
                        ".exe.lnk".IndexOf(ext, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        _loadingCache[ext] = icon;
                    }
                    return icon;
                }
            }
            return null;
        }
    
    }
}
