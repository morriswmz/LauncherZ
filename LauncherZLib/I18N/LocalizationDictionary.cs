using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace LauncherZLib.I18N
{
    /// <summary>
    /// A simple localization dictionary. 
    /// </summary>
    public class LocalizationDictionary
    {
        private static readonly List<string> PossibleCultureNames;

        private readonly List<string> _loadedLanguageFiles = new List<string>();
        private readonly Dictionary<string, string> _strings = new Dictionary<string, string>();
        

        private CultureInfo _culture = CultureInfo.CurrentCulture;

        public delegate void CultureChangedEventHandler(object sender, CultureChangedEventArgs e);

        public event CultureChangedEventHandler CultureChanged;

        static LocalizationDictionary()
        {
            PossibleCultureNames = CultureInfo.GetCultures(CultureTypes.AllCultures)
                    .Select(c => c.Name.ToLowerInvariant()).ToList();
        }

        public LocalizationDictionary()
        {

        }

        /// <summary>
        /// Gets the translated string.
        /// </summary>
        /// <param name="strName"></param>
        /// <returns>Translated string, or original string when fails.</returns>
        public string this[string strName]
        {
            get { return Translate(strName); }
        }

        /// <summary>
        /// Gets the current culture associated with the LocalizationDictionary.
        /// </summary>
        public CultureInfo CurrentCulture
        {
            get { return _culture; }
            set
            {
                if (!value.Equals(_culture))
                {
                    CultureInfo oldCultrue = _culture;
                    _culture = value;
                    ReloadAllLanguageFiles();
                    CultureChangedEventHandler handler = CultureChanged;
                    if (handler != null)
                        handler(null, new CultureChangedEventArgs(_culture, (CultureInfo)value.Clone()));
                }    
            }
        }

        /// <summary>
        /// Translates given string.
        /// </summary>
        /// <param name="strName"></param>
        /// <returns></returns>
        public string Translate(string strName)
        {
            return !_strings.ContainsKey(strName) ? strName : _strings[strName];
        }

        /// <summary>
        /// Checks if given string can be translated.
        /// </summary>
        /// <param name="strName"></param>
        /// <returns></returns>
        public bool CanTranslate(string strName)
        {
            return _strings.ContainsKey(strName);
        }

        /// <summary>
        /// Loads specific language file with fallback.
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadLanguageFile(string fileName)
        {
            LoadLanguageFile(fileName, true);
        }

        /// <summary>
        /// Loads specific language file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fallback">If true, will attempt to load language file for fallback language.</param>
        public void LoadLanguageFile(string fileName, bool fallback)
        {
            string baseFileName = TrimCultureNameFromPath(fileName);
            if (_loadedLanguageFiles.Contains(baseFileName))
                return;
            
            string expectedFileName = AddCultureNameToPath(baseFileName, _culture);
            if (File.Exists(expectedFileName))
            {
                LoadLanguageFileImpl(expectedFileName);
                return;
            }
            if (!fallback)
            {
                throw new FileNotFoundException(string.Format(
                    "Unable to find localization file: {0}.", expectedFileName));
            }

            // check fallback
            var fallbackFileName = AddCultureNameToPath(baseFileName, _culture.GetConsoleFallbackUICulture());
            if (File.Exists(fallbackFileName))
            {
                LoadLanguageFileImpl(fallbackFileName);
                return;
            }
            // check en-US as last resort
            fallbackFileName = AddCultureNameToPath(baseFileName, CultureInfo.CreateSpecificCulture("en-US"));
            if (File.Exists(fallbackFileName))
            {
                LoadLanguageFileImpl(fallbackFileName);
                return;
            }

            // cannot load
            throw new FileNotFoundException(string.Format(
                "Unable to find localization file: {0}. Fallback files not found.", expectedFileName));
        }

        /// <summary>
        /// Reloads all language files.
        /// </summary>
        public void ReloadAllLanguageFiles()
        {
            _strings.Clear();
            _loadedLanguageFiles.ForEach(LoadLanguageFile);
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
            if (string.IsNullOrEmpty(ext))
                return path;
            if (PossibleCultureNames.IndexOf(ext.TrimStart('.').ToLowerInvariant()) >= 0)
            {
                // file extension is culture name
                if (string.IsNullOrEmpty(dirName))
                {
                    return Path.GetFileNameWithoutExtension(fileName);
                }
                return string.Format("{0}{1}{2}",
                    dirName, Path.DirectorySeparatorChar, Path.GetFileNameWithoutExtension(fileName));
            }
            // file name is fff.xxx, fff.en-US.xxx or fff.nnn.xxx
            string fileNameNoLastExt = Path.GetFileNameWithoutExtension(fileName);
            string cultureName = Path.GetExtension(fileNameNoLastExt);
            if (string.IsNullOrEmpty(cultureName))
                return path;
            if (PossibleCultureNames.IndexOf(cultureName.TrimStart('.').ToLowerInvariant()) >= 0)
            {
                // file name is of format fff.en-US.xxx
                if (string.IsNullOrEmpty(dirName))
                {
                    return string.Format("{0}{1}", Path.GetFileNameWithoutExtension(fileNameNoLastExt), ext);
                }
                return string.Format("{0}{1}{2}{3}",
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
            if (string.IsNullOrEmpty(ext))
            {
                // no extension
                if (string.IsNullOrEmpty(dirName))
                {
                    return string.Format("{0}.{1}", fileName, culture.Name);
                }
                return string.Format("{0}{1}{2}.{3}",
                    dirName, Path.DirectorySeparatorChar, fileName, culture.Name);
            }
            else
            {
                // has extension
                if (string.IsNullOrEmpty(dirName))
                    return string.Format("{0}.{1}{2}",
                        Path.GetFileNameWithoutExtension(fileName),
                        culture.Name, ext);
                return string.Format("{0}{1}{2}.{3}{4}",
                    dirName, Path.DirectorySeparatorChar,
                    Path.GetFileNameWithoutExtension(fileName),
                    culture.Name, ext);
            }
        }

        private void LoadLanguageFileImpl(string fileName)
        {
            using (var sr = new StreamReader(fileName))
            {
                var jr = new JsonTextReader(sr);
                jr.Read();
                if (jr.TokenType != JsonToken.StartObject)
                    throw new FormatException("{ expected at beginning.");
                while (jr.Read())
                {
                    if (jr.TokenType == JsonToken.EndObject)
                    {
                        // end
                        break;
                    }
                    if (jr.TokenType == JsonToken.PropertyName)
                    {
                        // property
                        var propName = (string)jr.Value;
                        jr.Read();
                        if (jr.TokenType != JsonToken.String)
                            throw new FormatException(string.Format("None string value at line {0}:{1}.",
                                jr.LineNumber, jr.LinePosition));
                        _strings[propName] = (string)jr.Value;
                    }
                    else if (jr.TokenType == JsonToken.Comment)
                    {
                        // do nothing
                    }
                    else
                    {
                        throw new FormatException(string.Format("Unexpected token at line {0}:{1}",
                            jr.LineNumber, jr.LinePosition));
                    }
                }
            }   
        }

        

    }

    public class CultureChangedEventArgs : EventArgs
    {
        public CultureInfo OldCulture { get; private set; }
        public CultureInfo NewCulture { get; private set; }

        public CultureChangedEventArgs(CultureInfo oldCulture, CultureInfo newCulture)
        {
            OldCulture = oldCulture;
            NewCulture = newCulture;
        }
    }

    
}
