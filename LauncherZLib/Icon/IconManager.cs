using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using LauncherZLib.API;
using LauncherZLib.Utils;

namespace LauncherZLib.Icon
{
    public class IconManager
    {

        public delegate void IconLoadedHandler(IconLocation location, BitmapImage icon);

        private Dictionary<IconLocation, BitmapImage> _persistentIcons = new Dictionary<IconLocation, BitmapImage>();
        private SimpleCache<IconLocation, BitmapImage> _cachedIcons;
        private ConcurrentQueue<IconLocation> _pending = new ConcurrentQueue<IconLocation>();
        private Dictionary<IconLocation, List<IconLoadedHandler>> _handlers = new Dictionary<IconLocation, List<IconLoadedHandler>>();
        private IIconLocationResolver _resolver;
        private readonly object _loadLock = new object();

        public IconManager(int cacheCapacity, IIconLocationResolver resolver)
        {
            _resolver = resolver;
            _cachedIcons = new SimpleCache<IconLocation, BitmapImage>(cacheCapacity);
        }

        public BitmapImage DefaultIcon { get; set; }

        public bool ContainsIcon(IconLocation location)
        {
            lock (_loadLock)
            {
                return _persistentIcons.ContainsKey(location) || _cachedIcons.ContainsKey(location);
            }
        }

        public BitmapImage GetIcon(IconLocation location)
        {
            lock (_loadLock)
            {
                if (_persistentIcons.ContainsKey(location))
                    return _persistentIcons[location];
                if (_cachedIcons.ContainsKey(location))
                    return _cachedIcons[location];
                return DefaultIcon;
            }
        }

        public void RegisterPersistentIcon(IconLocation location, BitmapImage icon)
        {
            if (!icon.IsFrozen)
            {
                icon.Freeze();
            }
            lock (_loadLock)
            {
                _persistentIcons[location] = icon;
            }
        }

        public BitmapImage Load(IconLocation location, bool persistent)
        {
            throw new NotImplementedException();
        }

        public async void LoadAsync(IconLocation location, bool persistent, IconLoadedHandler callback)
        {
            bool initLoad = false;
            lock (_loadLock)
            {
                // check if icon is already loaded
                if (_persistentIcons.ContainsKey(location))
                {
                    callback(location, _persistentIcons[location]);
                    return;
                }
                if (_cachedIcons.ContainsKey(location))
                {
                    callback(location, _cachedIcons[location]);
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
                BitmapImage icon = await Task.Run(() => LoadImpl(location)) ?? DefaultIcon;
                lock (_loadLock)
                {
                    if (persistent)
                        _persistentIcons[location] = icon;
                    else
                        _cachedIcons[location] = icon;
                    foreach (var handler in _handlers[location])
                        handler(location, icon);
                    _handlers.Remove(location);
                }
            }
        }

        private BitmapImage LoadImpl(IconLocation location)
        {
            return null;
        }
    
    }
}
