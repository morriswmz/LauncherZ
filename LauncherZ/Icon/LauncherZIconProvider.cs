using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using LauncherZLib.Icon;

namespace LauncherZ.Icon
{
    public sealed class LauncherZIconProvider : IIconProvider, IIconRegisterer
    {

        private Dictionary<IconLocation, BitmapSource> _icons = new Dictionary<IconLocation, BitmapSource>(); 

        public BitmapSource ProvideIcon(IconLocation location)
        {
            BitmapSource icon;
            return _icons.TryGetValue(location, out icon) ? icon : null;
        }

        public IconAvailability GetIconAvailability(IconLocation location)
        {
            return _icons.ContainsKey(location) ? IconAvailability.Available : IconAvailability.NotAvailable;
        }

        public void RegisterIcon(IconLocation location, BitmapSource icon)
        {
            if (location == null)
                throw new ArgumentNullException("location");
            if (icon == null)
                throw new ArgumentNullException("location");
            _icons[location] = icon;
        }

    }
}
