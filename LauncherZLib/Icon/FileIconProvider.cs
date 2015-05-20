using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LauncherZLib.Utils;

namespace LauncherZLib.Icon
{
    /// <summary>
    /// Provides file icons.
    /// </summary>
    public sealed class FileIconProvider : IIconProvider
    {
        // special path
        private static readonly string DirectoryPath = ".";
        private static readonly string GenericFileName = "*";
        private static readonly string SpecialFileExtensions = ".LNK.EXE";

        // maps icon locations to cached icons
        private readonly SimpleCache<string, BitmapSource> _iconCache = new SimpleCache<string, BitmapSource>(128);
        // stores loaded icons, several icon locations may map to the same icon (e.g. file type icons) 
        private readonly SimpleCache<string, BitmapSource> _loadingCache = new SimpleCache<string, BitmapSource>(128); 
        private readonly object _cacheLock = new object();
        // logger instance
        private readonly ILogger _logger;

        public FileIconProvider(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            _logger = logger;
        }

        /// <summary>
        /// Gets or sets the icon for missing file.
        /// </summary>
        public BitmapSource MissingFileIcon { get; set; }

        /// <summary>
        /// Gets or sets the border brush for thumbnails.
        /// </summary>
        public Brush ThumbnailBorderBrush { get; set; }

        public BitmapSource ProvideIcon(IconLocation location)
        {
            if (location == null)
                throw new ArgumentNullException("location");
            string path = location.Path.Replace('/', '\\');

            BitmapSource icon;
            // check cache
            lock (_cacheLock)
            {
                if (_iconCache.TryGet(path, out icon))
                    return icon;
                if (_loadingCache.TryGet(path, out icon))
                {
                    _iconCache[path] = icon;
                    return icon;
                }
            }
            // check if given path is directory
            if (Directory.Exists(path))
            {
                // check loading cache for directory icon
                lock (_cacheLock)
                {
                    if (_loadingCache.TryGet(DirectoryPath, out icon))
                    {
                        // found, update icon cache
                        _iconCache[path] = icon;
                        return icon;
                    }
                }
                // not found, load it
                icon = SafeLoadDirectoryIcon();
                lock (_loadingCache)
                {
                    _loadingCache[DirectoryPath] = icon;
                    _iconCache[path] = icon;
                }
                return icon;
            }
            // check normal file
            if (File.Exists(path))
            {
                string ext = Path.GetExtension(path);
                // check image file
                if (IconLoader.IsSupportedImageFileExtension(ext))
                {
                    icon = SafeLoadThumbnailIcon(path);
                    lock (_cacheLock)
                    {
                        _loadingCache[path] = icon;
                        _iconCache[path] = icon;
                    }
                    return icon;
                }
                // check .exe, .lnk
                if (SpecialFileExtensions.IndexOf(ext, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    icon = SafeLoadFileIcon(path);
                    lock (_cacheLock)
                    {
                        _loadingCache[path] = icon;
                        _iconCache[path] = icon;
                    }
                    return icon;
                }
                // normal file
                string typeName = string.IsNullOrEmpty(ext) ? GenericFileName : ext;
                lock (_cacheLock)
                {
                    if (_loadingCache.TryGet(typeName, out icon))
                    {
                        _iconCache[path] = icon;
                        return icon;
                    }
                }
                icon = SafeLoadFileIcon(path);
                lock (_cacheLock)
                {
                    _loadingCache[typeName] = icon;
                    _iconCache[path] = icon;
                }
                return icon;
            }
            // not found
            lock (_cacheLock)
            {
                _iconCache[path] = MissingFileIcon;
            }
            return MissingFileIcon;
        }

        public IconAvailability GetIconAvailability(IconLocation location)
        {
            if (location == null)
                throw new ArgumentNullException("location");
            string path = location.Path.Replace('/', '\\');
            if (!location.Domain.Equals("file", StringComparison.OrdinalIgnoreCase))
                return IconAvailability.NotAvailable;
            // check cache first
            lock (_cacheLock)
            {
                if (_iconCache.ContainsKey(path) || _loadingCache.ContainsKey(path))
                    return IconAvailability.Available;
            }
            // we do not check file existence here
            string ext = Path.GetExtension(path);
            if (string.IsNullOrEmpty(ext))
            {
                // possible directory or file with no extension
                return (_loadingCache.ContainsKey(DirectoryPath) && _loadingCache.ContainsKey(GenericFileName))
                    ? IconAvailability.Available
                    : IconAvailability.AvailableLater;
            }
            // check normal file extension
            ext = ext.ToUpperInvariant();
            if (SpecialFileExtensions.IndexOf(ext, StringComparison.Ordinal) >= 0)
            {
                // usually .lnk and .exe files have distinct icons
                // since we have a cache miss, it will be available later
                return IconAvailability.AvailableLater;
            }
            // normal file, check file type
            lock (_cacheLock)
            {
                return _loadingCache.ContainsKey(GenericFileName + ext)
                    ? IconAvailability.Available
                    : IconAvailability.AvailableLater;
            }
        }

        private BitmapSource SafeLoadThumbnailIcon(string path)
        {
            try
            {
                return IconLoader.LoadThumbnail(path, IconSize.Large, ThumbnailBorderBrush);
            }
            catch (Exception ex)
            {
                _logger.Warning("Failed to generate thumbnail for: {0}. Details:{1}{2}", path, Environment.NewLine, ex);
                return MissingFileIcon;
            }
        }

        private BitmapSource SafeLoadFileIcon(string path)
        {
            try
            {
                return IconLoader.LoadFileIcon(path, IconSize.Large);
            }
            catch (Exception ex)
            {
                _logger.Warning("Failed to retrieve icon for: {0}. Details:{1}{2}", path, Environment.NewLine, ex);
                return MissingFileIcon;
            }
        }

        private BitmapSource SafeLoadDirectoryIcon()
        {
            try
            {
                return IconLoader.LoadDirectoryIcon(IconSize.Large);
            }
            catch (Exception ex)
            {
                _logger.Warning("Failed to generate directory icon. Details:{0}{1}", Environment.NewLine, ex);
                return MissingFileIcon;
            }
        }
    }
}
