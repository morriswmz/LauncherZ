using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace LauncherZLib.Icon
{
    /// <summary>
    /// <para>A simple static icon provider backed by a dictionary.</para>
    /// <para>Suitable for preloaded icons.</para>
    /// </summary>
    /// <remarks>
    /// This class is not thread-safe.
    /// </remarks>
    public class StaticIconProvider : IIconProvider, IIconRegisterer
    {
        protected readonly Dictionary<IconLocation, BitmapSource> _icons = new Dictionary<IconLocation, BitmapSource>(); 
        
        public BitmapSource ProvideIcon(IconLocation location)
        {
            BitmapSource icon;
            return _icons.TryGetValue(location, out icon) ? icon : null;
        }

        public virtual IconAvailability GetIconAvailability(IconLocation location)
        {
            return _icons.ContainsKey(location) ? IconAvailability.Available : IconAvailability.NotAvailable;
        }

        public virtual void RegisterIcon(IconLocation location, BitmapSource icon)
        {
            if (location == null)
                throw new ArgumentNullException("location");
            if (icon == null)
                throw new ArgumentNullException("icon");
            _icons[location] = icon;
        }
    }
}
