using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LauncherZ.Icon;

namespace LauncherZ.Controls
{
    class TaskIconBehavior
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
            var iconPath = e.NewValue as string;
            var image = d as Image;
            
            if (image == null)
                return;

            if (string.IsNullOrEmpty(iconPath))
            {
                image.Source = IconManager.DefaultIcon;
                return;
            }

            if (!IconManager.ContainsIcon(iconPath))
            {
                image.Source = IconManager.DefaultIcon;
                IconManager.ResolveIcon(iconPath, (success, icon) =>
                {
                    if (success)
                        image.Source = icon;
                });
            }
            else
            {
                image.Source = IconManager.GetIcon(iconPath);
            }
        }
    }
}
