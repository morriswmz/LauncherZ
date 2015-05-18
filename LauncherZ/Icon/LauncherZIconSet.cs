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
            Blank = iconRegisterer.RegisterIcon(app.FindResource("IconBlank") as BitmapSource);
            Program = iconRegisterer.RegisterIcon(app.FindResource("IconProgram") as BitmapSource);
        }

    }
}
