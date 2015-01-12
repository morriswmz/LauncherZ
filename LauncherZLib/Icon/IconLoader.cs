using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LauncherZLib.Win32;

namespace LauncherZLib.Icon
{

    public enum IconSize
    {
        /// <summary>
        /// 16 x 16 in pixels
        /// </summary>
        Small,
        /// <summary>
        /// 32 x 32 in pixels
        /// </summary>
        Large
    }

    public class IconLoader
    {

        private static readonly object _loadingLock = new object();

        public static bool IsSupportedImageFileExtension(string ext)
        {
            return ".png.jpg.bmp".IndexOf(ext, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static BitmapSource LoadDirectoryIcon(IconSize size)
        {
            return GetFileIcon(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), size);
        }

        public static BitmapSource LoadFileIcon(string path, IconSize size)
        {
            return File.Exists(path) ? GetFileIcon(path, size) : null;
        }

        public static BitmapSource LoadThumbnail(string path, IconSize size, Brush borderBrush)
        {
            return null;
        }

        private static BitmapSource GetFileIcon(string path, IconSize size)
        {
            lock (_loadingLock)
            {
                uint flags = Shell32.SHGFI_ICON;
                switch (size)
                {
                    case IconSize.Small:
                        flags |= Shell32.SHGFI_SMALLICON;
                        break;
                    case IconSize.Large:
                        flags |= Shell32.SHGFI_LARGEICON;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("size");
                }
                var sfi = new SHFILEINFO();
                IntPtr hResult = Shell32.SHGetFileInfo(path, 0u, ref sfi, (uint)Marshal.SizeOf(sfi), flags);
                if (hResult == IntPtr.Zero)
                    return null;
                BitmapSource imgSource = Imaging.CreateBitmapSourceFromHIcon(sfi.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                imgSource.Freeze();
                User32.DestroyIcon(sfi.hIcon); // possible Win32Exception ignored
                return imgSource;
            }
        }

    }
}
