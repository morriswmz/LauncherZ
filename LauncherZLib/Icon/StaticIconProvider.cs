using System;
using System.Collections.Generic;
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
        protected readonly string DomainField;
        protected readonly Dictionary<IconLocation, BitmapSource> _icons = new Dictionary<IconLocation, BitmapSource>();

        public StaticIconProvider(string domain)
        {
            if (domain == null)
                throw new ArgumentNullException("domain");
            if (!domain.IsProperDomainName())
                throw new ArgumentException("Domain name contains illegal characters.");
            DomainField = domain;
        }

        public string Domain { get { return DomainField; } }

        public BitmapSource ProvideIcon(string path)
        {
            BitmapSource icon;
            return _icons.TryGetValue(path, out icon) ? icon : null;
        }

        public virtual IconAvailability GetIconAvailability(string path)
        {
            return _icons.ContainsKey(path) ? IconAvailability.Available : IconAvailability.NotAvailable;
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
