using System.Windows.Media.Imaging;

namespace LauncherZLib.Icon
{
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
        /// <returns></returns>
        BitmapSource ProvideIcon(IconLocation location);

        /// <summary>
        /// Gets the availability for specified icon.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        IconAvailability GetIconAvailability(IconLocation location);

    }
}
