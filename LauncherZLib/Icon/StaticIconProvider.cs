using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media.Imaging;
using LauncherZLib.Utils;

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
        protected int IdCounter = 0;
        protected readonly string DomainField;
        protected readonly Dictionary<int, BitmapSource> Icons = new Dictionary<int, BitmapSource>();

        public StaticIconProvider(string domain)
        {
            if (domain == null)
                throw new ArgumentNullException("domain");
            if (!domain.IsProperDomainName())
                throw new ArgumentException("Domain name contains illegal characters.");
            DomainField = domain;
        }

        public string Domain { get { return DomainField; } }

        public virtual BitmapSource ProvideIcon(IconLocation location)
        {
            int id = GetIconId(location);
            BitmapSource icon;
            return Icons.TryGetValue(id, out icon) ? icon : null;
        }

        public virtual IconAvailability GetIconAvailability(IconLocation location)
        {
            int id = GetIconId(location);
            return id >= 0 && Icons.ContainsKey(id) ? IconAvailability.Available : IconAvailability.NotAvailable;
        }

        public virtual IconLocation RegisterIcon(BitmapSource icon)
        {
            if (icon == null)
            {
                throw new ArgumentNullException("icon");
            }
            Icons[IdCounter] = icon;
            IdCounter++;
            return new IconLocation(DomainField, (IdCounter-1).ToString(CultureInfo.InvariantCulture));
        }

        protected virtual int GetIconId(IconLocation location)
        {
            if (!location.Domain.Equals(DomainField, StringComparison.OrdinalIgnoreCase))
            {
                return -1;
            }
            int id;
            return int.TryParse(location.Path, out id) ? id : -1;
        }

    }
}
