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

        private readonly List<Tuple<string, string>> _loadedLanguageFiles = new List<Tuple<string, string>>();
        private readonly Dictionary<string, Dictionary<string, string>> _strings = new Dictionary<string, Dictionary<string, string>>();
        private static readonly List<string> PossibleCultureNames;

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

        public string Translate(string domain, string strName)
        {
            if (!_strings.ContainsKey(domain))
                return strName;
            if (!_strings[domain].ContainsKey(strName))
                return strName;
            return _strings[domain][strName];
        }

        public void LoadLanguageFile(string domain, string fileName)
        {
            LoadLanguageFile(domain, fileName, true);
        }

        public void LoadLanguageFile(string domain, string fileName, bool fallback)
        {
            string baseFileName = TrimCultureNameFromPath(fileName);
            string expectedFileName = AddCultureNameToPath(baseFileName, _culture);
            if (File.Exists(expectedFileName))
            {
                LoadLanguageFileImpl(domain, expectedFileName);
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
                LoadLanguageFileImpl(domain, fallbackFileName);
            }
            // check en-US as last resort
            fallbackFileName = AddCultureNameToPath(baseFileName, CultureInfo.InvariantCulture);
            if (File.Exists(fallbackFileName))
            {
                LoadLanguageFileImpl(domain, fallbackFileName);
            }

            // cannot load
            throw new FileNotFoundException(string.Format(
                "Unable to find localization file: {0}. Fallback files not found.", expectedFileName));
        }

        public void ReloadAllLanguageFiles()
        {
            _strings.Clear();
            _loadedLanguageFiles.ForEach(t => LoadLanguageFile(t.Item1, t.Item2));
        }

        
        public static string TrimCultureNameFromPath(string path)
        {
            string fileName = Path.GetFileName(path);
            string dirName = Path.GetDirectoryName(path);
            string ext = Path.GetExtension(fileName);
            // check existing extension
            if (string.IsNullOrEmpty(ext))
                return path;
            if (PossibleCultureNames.IndexOf(ext.ToLowerInvariant()) >= 0)
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
            if (PossibleCultureNames.IndexOf(cultureName.ToLowerInvariant()) >= 0)
            {
                // file name is of format fff.en-US.xxx
                if (string.IsNullOrEmpty(dirName))
                {
                    return string.Format("{0}.{1}", Path.GetFileNameWithoutExtension(fileNameNoLastExt), ext);
                }
                return string.Format("{0}{1}{2}.{3}",
                    dirName, Path.DirectorySeparatorChar,
                    Path.GetFileNameWithoutExtension(fileNameNoLastExt), ext);
            }
            return path;
        }

        public static string AddCultureNameToPath(string path, CultureInfo culture)
        {
            string fileName = Path.GetFileName(path);
            string dirName = Path.GetDirectoryName(path);
            string ext = Path.GetExtension(fileName);
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
                    return string.Format("{0}.{1}.{2}",
                        Path.GetFileNameWithoutExtension(fileName),
                        culture.Name, ext);
                return string.Format("{0}{1}{2}.{3}.{4}",
                    dirName, Path.DirectorySeparatorChar,
                    Path.GetFileNameWithoutExtension(fileName),
                    culture.Name, ext);
            }
        }

        private void LoadLanguageFileImpl(string domain, string fileName)
        {
            using (var sr = new StreamReader(fileName))
            {
                var jr = new JsonTextReader(sr);
                bool started = false, ended = false;
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
                        if (!_strings.ContainsKey(domain))
                            _strings.Add(domain, new Dictionary<string, string>());
                        _strings[domain][propName] = (string)jr.Value;
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
