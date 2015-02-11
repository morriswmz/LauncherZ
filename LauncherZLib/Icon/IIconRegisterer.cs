using System.Windows.Media.Imaging;

namespace LauncherZLib.Icon
{
    public interface IIconRegisterer
    {

        void RegisterIcon(IconLocation location, BitmapSource icon);

    }
}
