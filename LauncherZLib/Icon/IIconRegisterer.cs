using System.Windows.Media.Imaging;

namespace LauncherZLib.Icon
{
    /// <summary>
    /// Describes a class that supports icon registration.
    /// </summary>
    public interface IIconRegisterer
    {
        /// <summary>
        /// Registers an icon.
        /// </summary>
        /// <param name="icon">Icon to be registered.</param>
        /// <returns>Assigned icon location for this icon.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// Thrown when icon is null.
        /// </exception>
        IconLocation RegisterIcon(BitmapSource icon);

    }
}
