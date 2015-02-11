using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace LauncherZLib.Icon
{
    public sealed class IconLibrary
    {

        private readonly List<IIconProvider> _providers = new List<IIconProvider>();

        /// <summary>
        /// Gets or sets the default icon.
        /// This property can be null.
        /// </summary>
        public BitmapSource DefaultIcon { get; set; }


        public IconAvailability GetIconAvailability(IconLocation location)
        {
            return _providers.Select(iconProvider => iconProvider.GetIconAvailability(location))
                .FirstOrDefault(availability => availability != IconAvailability.NotAvailable);
        }

        public BitmapSource GetIcon(IconLocation location)
        {
            foreach (var iconProvider in _providers)
            {
                if (iconProvider.GetIconAvailability(location) != IconAvailability.NotAvailable)
                    return iconProvider.ProvideIcon(location);
            }
            return DefaultIcon;
        }

        public void GetIconAsync(IconLocation location, Action<IconLocation, BitmapSource> callback)
        {
            BitmapSource icon = null;
            IIconProvider provider;
            var availability = IconAvailability.NotAvailable;
            foreach (var p in _providers)
            {
                availability = p.GetIconAvailability(location);
                if (availability != IconAvailability.NotAvailable)
                {
                    provider = p;
                    break;
                }
            }
            switch (availability)
            {
                case IconAvailability.NotAvailable:
                    break;
                case IconAvailability.Available:
                    break;
                default:
                    break;
            }
            
        }

        public void RegisterProvider(IIconProvider provider)
        {
            
        }

        public void RemoveProvider(IIconProvider provider)
        {
            
        }

    }
}
