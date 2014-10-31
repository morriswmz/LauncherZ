using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LauncherZ.Icon
{
    public static class IconManager
    {

        private static readonly Lazy<Dictionary<string, BitmapImage>> InternalIcons;
        private static readonly Dictionary<string, List<Action<bool, BitmapImage>>> CallbackMap = new Dictionary<string, List<Action<bool, BitmapImage>>>();

        static IconManager()
        {
            InternalIcons = new Lazy<Dictionary<string, BitmapImage>>(() =>
            {
                var dict = new Dictionary<string, BitmapImage>();
                AddInternalIcon(dict, "IconProgram");
                AddInternalIcon(dict, "IconGear");
                AddInternalIcon(dict, "IconNetwork");
                AddInternalIcon(dict, "IconCalculator");
                return dict;
            });
        }

        public static BitmapImage DefaultIcon
        {
            get { return InternalIcons.Value["IconProgram"]; }
        }

        public static BitmapImage GetIcon(string iconLocation)
        {
            var location = new IconLocation(iconLocation);
            switch (location.Domain)
            {
                case IconDomain.Internal:
                    return InternalIcons.Value.ContainsKey(location.Path) ?
                        InternalIcons.Value[location.Path] : DefaultIcon;
                case IconDomain.External:
                case IconDomain.ExternalEmbedded:
                    // todo: retrieve from cache
                    return DefaultIcon;
                default:
                    return DefaultIcon;
            }
        }

        public static bool ContainsIcon(string iconLocation)
        {
            var location = new IconLocation(iconLocation);
            switch (location.Domain)
            {
                case IconDomain.Internal:
                    return InternalIcons.Value.ContainsKey(location.Path);
                case IconDomain.External:
                case IconDomain.ExternalEmbedded:
                    return false;
                default:
                    return false;
            }
        }

        public static void ResolveIcon(string iconLocation, Action<bool, BitmapImage> callback)
        {
            var location = new IconLocation(iconLocation);
            switch (location.Domain)
            {
                case IconDomain.Internal:
                    // fire call back immediately
                    if (InternalIcons.Value.ContainsKey(location.Path))
                        callback(true, InternalIcons.Value[location.Path]);
                    else
                        callback(false, DefaultIcon);
                    break;
                case IconDomain.External:
                    break;
                case IconDomain.ExternalEmbedded:
                    break;
                case IconDomain.Unknown:
                    callback(false, DefaultIcon);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void AddInternalIcon(Dictionary<string, BitmapImage> dict, string iconName)
        {
            var bitmapImage = Application.Current.FindResource(iconName) as BitmapImage;
            if (dict.ContainsKey(iconName))
            {
                dict[iconName] = bitmapImage;
            }
            else
            {
                dict.Add(iconName, bitmapImage);
            }
        }

    }
}
