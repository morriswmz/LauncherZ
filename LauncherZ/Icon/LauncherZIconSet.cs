using System.Windows.Media.Imaging;
using LauncherZLib.Icon;

namespace LauncherZ.Icon
{
    public static class LauncherZIconSet
    {
        public static IconLocation Blank { get; private set; }
        public static IconLocation Program { get; private set; }
        public static IconLocation Gear { get; private set; }
        public static IconLocation Network { get; private set; }
        public static IconLocation Calculator { get; private set; }

        internal static void RegisterIconSet(LauncherZApp app, IIconRegisterer iconRegisterer)
        {
            // "IconProgram", "IconGear", "IconNetwork", "IconCalculator", "IconFolder", "IconBlank"
            Blank = RegisterIcon(iconRegisterer, app.FindResource("IconBlank") as BitmapSource);
            Program = RegisterIcon(iconRegisterer, app.FindResource("IconProgram") as BitmapSource);
            Gear = RegisterIcon(iconRegisterer, app.FindResource("IconGear") as BitmapSource);
            Network = RegisterIcon(iconRegisterer, app.FindResource("IconNetwork") as BitmapSource);
            Calculator = RegisterIcon(iconRegisterer, app.FindResource("IconCalculator") as BitmapSource);
        }

        internal static IconLocation RegisterIcon(IIconRegisterer iconRegisterer, BitmapSource icon)
        {
            return icon == null ? IconLocation.NotFound : iconRegisterer.RegisterIcon(icon);
        }

    }
}
