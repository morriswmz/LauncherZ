using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using LauncherZLib.Icon;

namespace LauncherZ.Behaviors
{
    internal static class LauncherIconBehavior
    {
        public static readonly DependencyProperty IconLocationProperty =
            DependencyProperty.RegisterAttached("IconLocation", typeof(string), typeof(LauncherIconBehavior), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnIconLocationChanged));
     
        public static void SetIconLocation(Image d, string value)
        {
            d.SetValue(IconLocationProperty, value);
        }


        public static string GetIconLocation(Image d)
        {
            return (string) d.GetValue(IconLocationProperty);
        }

        private static void OnIconLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Image image;
            if (DesignerProperties.GetIsInDesignMode(d))
            {
                image = d as Image;
                if (image != null)
                    image.Source = Application.Current.FindResource("IconBlank") as BitmapImage;
                return;
            }

            IconLibrary iconLibrary = LauncherZApp.Instance == null ? null : LauncherZApp.Instance.IconLibrary;
            if (iconLibrary == null)
            {
                return;
            }

            var iconLocationStr = e.NewValue as string;
            image = d as Image;
            if (image == null)
                return;

            if (string.IsNullOrEmpty(iconLocationStr))
            {
                image.Source = iconLibrary.DefaultIcon;
                return;
            }

            var iconLocation = new IconLocation(iconLocationStr);

            switch (iconLibrary.GetIconAvailability(iconLocation))
            {
                case IconAvailability.NotAvailable:
                    image.Source = iconLibrary.DefaultIcon;
                    break;
                case IconAvailability.Available:
                    image.Source = iconLibrary.GetIcon(iconLocation);
                    break;
                case IconAvailability.AvailableLater:
                    iconLibrary.GetIconAsync(iconLocation, (location, icon) =>
                    {
                        if (location.Equals(new IconLocation(GetIconLocation(image))))
                            image.Source = icon;
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
