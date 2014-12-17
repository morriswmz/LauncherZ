using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LauncherZLib.Icon;

namespace LauncherZ.Controls
{
    public static class TaskIconBehavior
    {


        public static readonly DependencyProperty IconLocationProperty =
            DependencyProperty.RegisterAttached("IconLocation", typeof(string), typeof(TaskIconBehavior), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.AffectsRender, OnIconLocationChanged));
     
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
            IconManager iconManager = LauncherZApp.Instance == null ? null : LauncherZApp.Instance.IconManager;
            if (iconManager == null)
            {
                return;
            }

            var iconLocationStr = e.NewValue as string;
            var image = d as Image;
            
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
                iconManager.LoadAsync(iconLocation, false, (location, icon) =>
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
