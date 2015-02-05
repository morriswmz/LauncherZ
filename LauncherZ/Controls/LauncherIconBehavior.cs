using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using LauncherZLib.Icon;

namespace LauncherZ.Controls
{
    public static class LauncherIconBehavior
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

            IconManager iconManager = LauncherZApp.Instance == null ? null : LauncherZApp.Instance.IconManager;
            if (iconManager == null)
            {
                return;
            }

            var iconLocationStr = e.NewValue as string;
            image = d as Image;
            if (image == null)
                return;

            if (string.IsNullOrEmpty(iconLocationStr))
            {
                image.Source = iconManager.DefaultIcon;
                return;
            }

            var iconLocation = new IconLocation(iconLocationStr);

            if (!iconManager.ContainsIcon(iconLocation))
            {
                image.Source = iconManager.DefaultIcon;
                iconManager.AddIconAsync(iconLocation, false, (location, icon) =>
                {
                    if (location.ToString() == GetIconLocation(image))
                        image.Source = icon;
                });
            }
            else
            {
                image.Source = iconManager.GetIcon(iconLocation);
            }
        }
    }
}
