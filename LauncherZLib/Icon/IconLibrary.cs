using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace LauncherZLib.Icon
{
    /// <summary>
    /// Provides access to icons provided by <see cref="T:LauncherZLib.Icon.IIconProvider"/>.
    /// </summary>
    /// <remarks>
    /// This class is not thread-safe and should be used only in main UI thread .
    /// </remarks>
    public sealed class IconLibrary
    {

        private readonly List<IIconProvider> _providers = new List<IIconProvider>();
        private readonly Dictionary<IconLocation, List<Action<IconLocation, BitmapSource>>> _callbacks =
            new Dictionary<IconLocation, List<Action<IconLocation, BitmapSource>>>();

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
            return _providers.Select(iconProvider => iconProvider.GetIconAvailability(location))
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
            foreach (var iconProvider in _providers)
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
        public async void GetIconAsync(IconLocation location, Action<IconLocation, BitmapSource> callback)
        {
            // register callback
            List<Action<IconLocation, BitmapSource>> callbackList;
            if (_callbacks.TryGetValue(location, out callbackList))
            {
                callbackList.Add(callback);
            }
            else
            {
                callbackList = new List<Action<IconLocation, BitmapSource>> {callback};
                _callbacks[location] = callbackList;
            }
            // get icon
            IIconProvider provider = null;
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

        /// <summary>
        /// <para>Registers an icon provider.</para>
        /// <para>Does nothing if given icon provider is already registered.</para>
        /// </summary>
        /// <param name="provider"></param>
        public void RegisterProvider(IIconProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");
            if (!_providers.Contains(provider))
            {
                _providers.Add(provider);
            }
        }

        /// <summary>
        /// <para>Removes an icon provider.</para>
        /// <para>Does nothing if given icon provider is not registered.</para>
        /// </summary>
        /// <param name="provider"></param>
        public void RemoveProvider(IIconProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");
            _providers.Remove(provider);
        }

    }
}
