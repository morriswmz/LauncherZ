using System;
using System.IO;
using System.Runtime.InteropServices;
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
            return !string.IsNullOrEmpty(ext) && ".PNG.JPG.BMP".IndexOf(ext, StringComparison.OrdinalIgnoreCase) >= 0;
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
            int sizeLimit = size == IconSize.Small ? 16 : 32;
            int borderThickness = size == IconSize.Small ? 1 : 2;
            var image = new BitmapImage();
            // read information without actual decoding
            image.BeginInit();
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.None;
            image.EndInit();
            image.Freeze();
            bool isLandScape = image.PixelWidth > image.PixelHeight;
            // now do the actual loading
            image = new BitmapImage();
            image.BeginInit();
            if (isLandScape)
            {
                image.DecodePixelHeight = sizeLimit;
            }
            else
            {
                image.DecodePixelWidth = sizeLimit;
            }
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.None;
            image.EndInit();
            image.Freeze(); // wpf is lazy, it is likely that the image is still not decoded
            // draw the thumbnail
            var drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                // draw image
                var imageRect = isLandScape
                    ? new Rect(-(image.PixelWidth - image.PixelHeight)/2.0, 0, image.PixelWidth, sizeLimit)
                    : new Rect(0, -(image.PixelHeight - image.PixelWidth) / 2.0, sizeLimit, image.PixelHeight);
                drawingContext.DrawImage(image, imageRect);
                // draw border
                var borderRect = new Rect(borderThickness / 2.0, borderThickness / 2.0,
                    sizeLimit - borderThickness, sizeLimit - borderThickness);
                drawingContext.DrawRectangle(Brushes.Transparent, new Pen(borderBrush, borderThickness), borderRect);
            }
            // render, defaults to 96dpi
            var renderTarget = new RenderTargetBitmap(sizeLimit, sizeLimit, 96.0, 96.0, PixelFormats.Pbgra32);
            renderTarget.Render(drawingVisual);
            renderTarget.Freeze();
            return renderTarget;
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
