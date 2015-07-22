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
    public class LocalizationDictionary : ILocalizationDictionary
    {
        private readonly List<string> _loadedLanguageFiles = new List<string>();
        private readonly Dictionary<string, string> _strings = new Dictionary<string, string>();
        

        private CultureInfo _culture = CultureInfo.CurrentCulture;

        public delegate void CultureChangedEventHandler(object sender, CultureChangedEventArgs e);

        public event CultureChangedEventHandler CultureChanged;

        public LocalizationDictionary()
        {

        }

        public string this[string strName]
        {
            get { return Translate(strName); }
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

        public string Translate(string strName)
        {
            return !_strings.ContainsKey(strName) ? strName : _strings[strName];
        }

        public bool CanTranslate(string strName)
        {
            return _strings.ContainsKey(strName);
        }

        public void LoadLanguageFile(string fileName)
        {
            LoadLanguageFile(fileName, true);
        }

        public void LoadLanguageFile(string fileName, bool fallback)
        {
            string baseFileName = LocalizationHelper.TrimCultureNameFromPath(fileName);
            if (_loadedLanguageFiles.Contains(baseFileName))
                return;
            
            string expectedFileName = LocalizationHelper.AddCultureNameToPath(baseFileName, _culture);
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
            var fallbackFileName = LocalizationHelper.AddCultureNameToPath(baseFileName, _culture.GetConsoleFallbackUICulture());
            if (File.Exists(fallbackFileName))
            {
                LoadLanguageFileImpl(fallbackFileName);
                return;
            }
            // check en-US as last resort
            fallbackFileName = LocalizationHelper.AddCultureNameToPath(baseFileName, CultureInfo.CreateSpecificCulture("en-US"));
            if (File.Exists(fallbackFileName))
            {
                LoadLanguageFileImpl(fallbackFileName);
                return;
            }

            // cannot load
            throw new FileNotFoundException(string.Format(
                "Unable to find localization file: {0}. Fallback files not found.", expectedFileName));
        }

        public void ReloadAllLanguageFiles()
        {
            _strings.Clear();
            _loadedLanguageFiles.ForEach(LoadLanguageFile);
        }

        public void Clear()
        {
            _strings.Clear();
            _loadedLanguageFiles.Clear();
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
