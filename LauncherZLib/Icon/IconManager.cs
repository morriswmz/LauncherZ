using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using LauncherZLib.Utils;

namespace LauncherZLib.Icon
{
    public class IconManager
    {

        private Dictionary<IconLocation, BitmapImage> _persistentIcons = new Dictionary<IconLocation, BitmapImage>();
        private SimpleCache<BitmapImage> _cachedIcons = new SimpleCache<BitmapImage>(512); 
        private ConcurrentQueue<IconLocation> _pending = new ConcurrentQueue<IconLocation>(); 

        public IconManager(int cacheCapacity)
        {
            
        }

        public bool ContainsIcon(IconLocation location)
        {
            
        }

        public BitmapImage GetIcon(IconLocation location)
        {
            
        }

        public void RegisterPersistentIcon(IconLocation location, BitmapImage icon)
        {
            if (!icon.IsFrozen)
            {
                icon.Freeze();
            }
            _persistentIcons[location] = icon;
        }

        public BitmapImage Load(IconLocation location, bool persistent)
        {
            
        }

        public void LoadAsync(IconLocation location, bool persistent, Action<IconLocation, BitmapImage> callback)
        {
            
        }

        private BitmapImage LoadImpl(IconLocation location)
        {
            
        }
   
    }
}
