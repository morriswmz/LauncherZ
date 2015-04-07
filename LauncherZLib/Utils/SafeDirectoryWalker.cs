using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LauncherZLib.Utils
{

   

    /// <summary>
    /// Walks through every file safely.
    /// </summary>
    public class SafeDirectoryWalker
    {

        private int _maxDepth = 1;

        public SafeDirectoryWalker()
        {
            IgnoreHiddenFiles = true;
            IgnoreUnixHiddenFiles = true;
            IgnoreTemporaryFiles = true;
            IgnoreSystemFiles = true;
            Recursive = false;
        }

        public delegate bool DirectoryWalkerCallback(FileInfo fi);
        /// <summary>
        /// Gets or gets whether hidden files/directories should be ignored.
        /// Default is true.
        /// </summary>
        public bool IgnoreHiddenFiles { get; set; }

        /// <summary>
        /// Gets or gets whether UNIX-like hidden files/directories (starts with ".") should be ignored.
        /// Default is true.
        /// </summary>
        public bool IgnoreUnixHiddenFiles { get; set; }

        /// <summary>
        /// Gets or gets whether temporary files/directories should be ignored.
        /// Default is true.
        /// </summary>
        public bool IgnoreTemporaryFiles { get; set; }

        /// <summary>
        /// Gets or gets whether system files/directories should be ignored.
        /// Default is true.
        /// </summary>
        public bool IgnoreSystemFiles { get; set; }

        /// <summary>
        /// Gets or gets the iteration should be recursive.
        /// </summary>
        public bool Recursive { get; set; }

        /// <summary>
        /// Gets or sets the search pattern for file names.
        /// </summary>
        public Regex SearchPattern { get; set; }

        /// <summary>
        /// Gets or sets the maximum directory depth.
        /// </summary>
        public int MaxDepth
        {
            get { return _maxDepth; }
            set { _maxDepth = Math.Max(1, value); }
        }

        /// <summary>
        /// Iterates through a directory.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="callback"></param>
        public void Walk(string directory, DirectoryWalkerCallback callback)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException(string.Format("Cannot find {0}", directory));

            if (callback == null)
                throw new ArgumentNullException();

            WalkImpl(new DirectoryInfo(directory), 1, callback);
        }

        private bool WalkImpl(DirectoryInfo cwd, int depth, DirectoryWalkerCallback callback)
        {
            try
            {
                // enumerate files
                IEnumerable<FileInfo> fe = cwd.EnumerateFiles();
                // do not use LINQ here. exception might be thrown.
                foreach (var fileInfo in fe)
                {
                    if (ShouldIncludeFile(fileInfo))
                    {
                        if (!callback(fileInfo))
                            return false;
                    }
                }
                // check depth
                if (depth >= MaxDepth || !Recursive)
                    return true;

                // enumerate subdirectories
                IEnumerable<DirectoryInfo> de = cwd.EnumerateDirectories();
                // do not use LINQ here, also
                foreach (var directoryInfo in de)
                {
                    if (ShouldIncludeDirectory(directoryInfo))
                    {
                        if (!WalkImpl(directoryInfo, depth + 1, callback))
                            return false;
                    }
                }
            }
            catch (Exception ex)
            {
                // safely ignore
            }
            return true;
        }

        private bool ShouldIncludeDirectory(DirectoryInfo di)
        {
            try
            {
                FileAttributes dirAttributes = di.Attributes;
                if (!CheckAttributes(dirAttributes))
                    return false;

                string dirName = di.Name;
                if (dirName.StartsWith(".") && IgnoreUnixHiddenFiles)
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool ShouldIncludeFile(FileInfo fi)
        {
            try
            {
                FileAttributes fileAttributes = fi.Attributes;
                if (!CheckAttributes(fileAttributes))
                    return false;

                string fileName = fi.Name;
                if (fileName.StartsWith(".") && IgnoreUnixHiddenFiles)
                    return false;

                if (SearchPattern != null && !SearchPattern.IsMatch(fileName))
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private bool CheckAttributes(FileAttributes attr)
        {
            // ignore reparse points (symbolic link, junction, etc)
            if (attr.HasFlag(FileAttributes.ReparsePoint))
                return false;
            if (attr.HasFlag(FileAttributes.Temporary) && IgnoreTemporaryFiles)
                return false;
            if (attr.HasFlag(FileAttributes.Hidden) && IgnoreHiddenFiles)
                return false;
            if (attr.HasFlag(FileAttributes.System) && IgnoreSystemFiles)
                return false;
            return true;
        }

    }
}
