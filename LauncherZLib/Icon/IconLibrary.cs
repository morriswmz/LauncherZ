using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using LauncherZLib.Utils;

namespace LauncherZLib.Icon
{
    /// <summary>
    /// Provides access to icons provided by <see cref="T:LauncherZLib.Icon.IIconProvider"/>.
    /// </summary>
    /// <remarks>
    /// This class is not thread-safe and should be used only in main UI thread .
    /// </remarks>
    /// TODO: each provider should only handle one domain
    public sealed class IconLibrary : IIconProviderRegistry
    {

        private readonly Dictionary<string, List<IIconProvider>> _providerMap =
            new Dictionary<string, List<IIconProvider>>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<IIconProvider> _registeredProviders = new HashSet<IIconProvider>(); 
        private readonly Dictionary<IconLocation, List<IconLoadedCallback>> _callbacks =
            new Dictionary<IconLocation, List<IconLoadedCallback>>();

        public delegate void IconLoadedCallback(IconLocation il, BitmapSource icon);

        /// <summary>
        /// Gets or sets the default icon.
        /// This property can be null.
        /// </summary>
        public BitmapSource DefaultIcon { get; set; }

        /// <summary>
        /// Checks the availability of specified icon.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public IconAvailability GetIconAvailability(IconLocation location)
        {
            if (!_providerMap.ContainsKey(location.Domain))
                return IconAvailability.NotAvailable;
            return _providerMap[location.Domain].Select(iconProvider => iconProvider.GetIconAvailability(location))
                .FirstOrDefault(availability => availability != IconAvailability.NotAvailable);
        }

        /// <summary>
        /// Gets specificed icon synchronously.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// This method will iterate through the provider list and pick the first one that is capable of
        /// providing the specified icon (i.e. <see cref="F:LauncheZLib.Icon.IconAvailability.Available"/>
        /// or <see cref="F:LauncheZLib.Icon.IconAvailability.AvailableLater"/>).
        /// </para>
        /// <para>
        /// Calling this method might freeze the UI since some icons are not immediately available.
        /// </para>
        /// </remarks>
        public BitmapSource GetIcon(IconLocation location)
        {
            if (!_providerMap.ContainsKey(location.Domain))
            {
                return DefaultIcon;
            }
            foreach (var iconProvider in _providerMap[location.Domain])
            {
                if (iconProvider.GetIconAvailability(location) != IconAvailability.NotAvailable)
                    return iconProvider.ProvideIcon(location);
            }
            return DefaultIcon;
        }

        /// <summary>
        /// Gets specified icon asynchronously.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="callback">
        /// An callback action that will be invoked when icon is available for use.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method will iterate through the provider list and pick the first one that is capable of
        /// providing the specified icon (i.e. <see cref="F:LauncheZLib.Icon.IconAvailability.Available"/>
        /// or <see cref="F:LauncheZLib.Icon.IconAvailability.AvailableLater"/>).
        /// </para>
        /// </remarks>
        public async void GetIconAsync(IconLocation location, IconLoadedCallback callback)
        {
            // register callback
            List<IconLoadedCallback> callbackList;
            if (_callbacks.TryGetValue(location, out callbackList))
            {
                callbackList.Add(callback);
            }
            else
            {
                callbackList = new List<IconLoadedCallback> { callback };
                _callbacks[location] = callbackList;
            }
            // get icon
            IIconProvider provider = null;
            var availability = IconAvailability.NotAvailable;
            foreach (var p in _providerMap[location.Domain])
            {
                availability = p.GetIconAvailability(location);
                if (availability != IconAvailability.NotAvailable)
                {
                    provider = p;
                    break;
                }
            }
            BitmapSource icon = null;
            switch (availability)
            {
                case IconAvailability.NotAvailable:
                    icon = DefaultIcon;
                    break;
                case IconAvailability.Available:
                    Debug.Assert(provider != null);
                    icon = provider.ProvideIcon(location);
                    break;
                case IconAvailability.AvailableLater:
                    Debug.Assert(provider != null);
                    // note that the closure captures both icon and location.
                    // since icon is not used here, we can safely ignore the warning
                    icon = await Task.Run(() => provider.ProvideIcon(location)) ?? DefaultIcon;
                    break;
            }
            // invoke callbacks
            if (_callbacks.TryGetValue(location, out callbackList))
            {
                callbackList.ForEach(f => f(location, icon));
                _callbacks.Remove(location);
            }
        }

        public bool IsDomainRegistered(string domain)
        {
            return _providerMap.ContainsKey(domain);
        }

        public void RegisterIconProvider(string domains, IIconProvider iconProvider)
        {
            if (domains == null)
                throw new ArgumentNullException("domains");
            if (iconProvider == null)
                throw new ArgumentNullException("iconProvider");
            // retrieve and check domain names
            string[] domainArr = domains.Split(',');
            foreach (var d in domainArr)
            {
                if (!d.IsProperDomainName())
                {
                    throw new ArgumentException(string.Format("{0} is not a valid domain name.", d));
                }
            }
            // remove existing registeration if possible
            if (_registeredProviders.Contains(iconProvider))
            {
                UnregisterIconProvider(iconProvider);
            }
            // assign new
            foreach (var d in domainArr)
            {
                if (!_providerMap.ContainsKey(d))
                {
                    _providerMap[d] = new List<IIconProvider>(4);
                }
                _providerMap[d].Add(iconProvider);
            }
        }

        public void UnregisterIconProvider(IIconProvider iconProvider)
        {
            if (iconProvider == null)
                throw new ArgumentNullException("iconProvider");
            foreach (var l in _providerMap.Values)
            {
                l.Remove(iconProvider);
            }
        }

    }
}
