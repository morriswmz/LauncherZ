using System.Windows.Media.Imaging;

namespace LauncherZLib.Icon
{
    /// <summary>
    /// Describes an icon provider.
    /// </summary>
    public interface IIconProvider
    {
        /// <summary>
        /// Gets the supported domains of this icon provider. Multiple domains are separated by comma.
        /// The domain name must begin with a letter, and contains only letters and digits.
        /// </summary>
        string Domain { get; }
        
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
        /// <returns></returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// Thrown when <paramref name="location"/> is null.
        /// </exception>
        BitmapSource ProvideIcon(IconLocation location);

        /// <summary>
        /// Gets the availability for specified icon.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// Thrown when <paramref name="location"/> is null.
        /// </exception>
        IconAvailability GetIconAvailability(IconLocation location);

    }
}
