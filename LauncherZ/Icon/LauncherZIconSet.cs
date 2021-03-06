﻿using System.Windows.Media.Imaging;
using LauncherZLib.Icon;

namespace LauncherZ.Icon
{
    public static class LauncherZIconSet
    {
        public static IconLocation Blank { get; private set; }
        public static IconLocation Exit { get; private set; }
        public static IconLocation Link { get; private set; }
        public static IconLocation Program { get; private set; }
        public static IconLocation Gear { get; private set; }
        public static IconLocation Network { get; private set; }
        public static IconLocation Calculator { get; private set; }
        public static IconLocation ComponentActive { get; private set; }
        public static IconLocation ComponentInactive { get; private set; }

        static LauncherZIconSet()
        {
            Blank = IconLocation.NotFound;
            Exit = IconLocation.NotFound;
            Link = IconLocation.NotFound;
            Program = IconLocation.NotFound;
            Gear = IconLocation.NotFound;
            Network = IconLocation.NotFound;
            Calculator = IconLocation.NotFound;
            ComponentActive = IconLocation.NotFound;
            ComponentInactive = IconLocation.NotFound;
        }

        internal static void RegisterIconSet(LauncherZApp app, IIconRegisterer iconRegisterer)
        {
            // "IconProgram", "IconGear", "IconNetwork", "IconCalculator", "IconFolder", "IconBlank"
            Blank = RegisterIcon(iconRegisterer, app.FindResource("IconBlank") as BitmapSource);
            Exit = RegisterIcon(iconRegisterer, app.FindResource("IconExit") as BitmapSource);
            Link = RegisterIcon(iconRegisterer, app.FindResource("IconLink") as BitmapSource);
            Program = RegisterIcon(iconRegisterer, app.FindResource("IconProgram") as BitmapSource);
            Gear = RegisterIcon(iconRegisterer, app.FindResource("IconGear") as BitmapSource);
            Network = RegisterIcon(iconRegisterer, app.FindResource("IconNetwork") as BitmapSource);
            Calculator = RegisterIcon(iconRegisterer, app.FindResource("IconCalculator") as BitmapSource);
            ComponentActive = RegisterIcon(iconRegisterer, app.FindResource("IconComponentActive") as BitmapSource);
            ComponentInactive = RegisterIcon(iconRegisterer, app.FindResource("IconComponentInactive") as BitmapSource);
        }

        internal static IconLocation RegisterIcon(IIconRegisterer iconRegisterer, BitmapSource icon)
        {
            return icon == null ? IconLocation.NotFound : iconRegisterer.RegisterIcon(icon);
        }

    }
}
