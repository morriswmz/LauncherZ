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
        /// <param name="location"></param>
        /// <param name="icon"></param>
        /// <exception cref="T:System.ArgumentNullException">
        /// Thrown when any argument is null.
        /// </exception>
        void RegisterIcon(IconLocation location, BitmapSource icon);

    }
}
