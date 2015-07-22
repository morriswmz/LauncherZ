using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace LauncherZLib.I18N
{
    public static class LocalizationHelper
    {
        private static readonly List<string> PossibleCultureNames;

        static LocalizationHelper()
        {
            PossibleCultureNames = CultureInfo.GetCultures(CultureTypes.AllCultures)
                    .Select(c => c.Name.ToLowerInvariant()).ToList();
        }

        /// <summary>
        /// Removes culture name from given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <example>
        /// Strings.json             -> Strings.json
        /// Strings.en-US.json       -> Strings.json
        /// Strings.en-US.zh-CN.json -> Strings.en-US.json
        /// </example>
        public static string TrimCultureNameFromPath(string path)
        {
            string fileName = Path.GetFileName(path);
            string dirName = Path.GetDirectoryName(path);
            string ext = Path.GetExtension(fileName);
            // check existing extension
            if (String.IsNullOrEmpty(ext))
                return path;
            if (PossibleCultureNames.IndexOf(ext.TrimStart('.').ToLowerInvariant()) >= 0)
            {
                // file extension is culture name
                if (String.IsNullOrEmpty(dirName))
                {
                    return Path.GetFileNameWithoutExtension(fileName);
                }
                return String.Format("{0}{1}{2}",
                    dirName, Path.DirectorySeparatorChar, Path.GetFileNameWithoutExtension(fileName));
            }
            // file name is fff.xxx, fff.en-US.xxx or fff.nnn.xxx
            string fileNameNoLastExt = Path.GetFileNameWithoutExtension(fileName);
            string cultureName = Path.GetExtension(fileNameNoLastExt);
            if (String.IsNullOrEmpty(cultureName))
                return path;
            if (PossibleCultureNames.IndexOf(cultureName.TrimStart('.').ToLowerInvariant()) >= 0)
            {
                // file name is of format fff.en-US.xxx
                if (String.IsNullOrEmpty(dirName))
                {
                    return String.Format("{0}{1}", Path.GetFileNameWithoutExtension(fileNameNoLastExt), ext);
                }
                return String.Format("{0}{1}{2}{3}",
                    dirName, Path.DirectorySeparatorChar,
                    Path.GetFileNameWithoutExtension(fileNameNoLastExt), ext);
            }
            return path;
        }

        /// <summary>
        /// Append culture name to given path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <example>
        /// Strings            -> Strings.en-US
        /// Strings.json       -> Strings.en-US.json
        /// Strings.en-US.json -> Strings.en-US.en-US.json
        /// </example>
        public static string AddCultureNameToPath(string path, CultureInfo culture)
        {
            string fileName = Path.GetFileName(path);
            string dirName = Path.GetDirectoryName(path);
            string ext = Path.GetExtension(fileName); // note the dot is not removed!
            if (String.IsNullOrEmpty(ext))
            {
                // no extension
                if (String.IsNullOrEmpty(dirName))
                {
                    return String.Format("{0}.{1}", fileName, culture.Name);
                }
                return String.Format("{0}{1}{2}.{3}",
                    dirName, Path.DirectorySeparatorChar, fileName, culture.Name);
            }
            else
            {
                // has extension
                if (String.IsNullOrEmpty(dirName))
                    return String.Format("{0}.{1}{2}",
                        Path.GetFileNameWithoutExtension(fileName),
                        culture.Name, ext);
                return String.Format("{0}{1}{2}.{3}{4}",
                    dirName, Path.DirectorySeparatorChar,
                    Path.GetFileNameWithoutExtension(fileName),
                    culture.Name, ext);
            }
        }
    }
}
