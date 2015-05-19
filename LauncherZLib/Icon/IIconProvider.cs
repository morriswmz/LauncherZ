using System.Windows.Media.Imaging;

namespace LauncherZLib.Icon
{
    /// <summary>
    /// Describes an icon provider.
    /// </summary>
    public interface IIconProvider
    {
        /// <summary>
        /// <para>
        /// Provides specified icon.
        /// </para>
        /// <para>
        /// This method should be implemented synchronously.
        /// <see cref="T:LauncherZLib.Icon.IconLibrary"/> will automatical spawn tasks for icons
        /// with availability of <see cref="F:LauncherZLib.Icon.IconAvailability.AvailableLater"/>.
        /// </para>
        /// </summary>
        /// <param name="location"></param>
        /// <returns>
        /// Corresponding bitmap source. Null is permitted.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// Thrown when <paramref name="location"/> is null.
        /// </exception>
        BitmapSource ProvideIcon(IconLocation location);

        /// <summary>
        /// Gets the availability for specified icon.
        /// </summary>
        /// <param name="location"></param>
        /// <returns>
        /// See <see cref="T:LauncherZLib.Icon.IconAvailability"/> for details.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// Thrown when <paramref name="location"/> is null.
        /// </exception>
        IconAvailability GetIconAvailability(IconLocation location);

    }
}
