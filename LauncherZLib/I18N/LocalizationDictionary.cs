﻿using System;
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

        public string this[string strName]
        {
            get { return Translate(strName); }
        }

        /// <summary>
        /// 
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
        /// 
        /// </summary>
        /// <param name="strName"></param>
        /// <returns></returns>
        public string Translate(string strName)
        {
            if (!_strings.ContainsKey(strName))
                return strName;
            return _strings[strName];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strName"></param>
        /// <returns></returns>
        public bool CanTranslate(string strName)
        {
            return _strings.ContainsKey(strName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadLanguageFile(string fileName)
        {
            LoadLanguageFile(fileName, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fallback"></param>
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
            }
            // check en-US as last resort
            fallbackFileName = AddCultureNameToPath(baseFileName, CultureInfo.InvariantCulture);
            if (File.Exists(fallbackFileName))
            {
                LoadLanguageFileImpl(fallbackFileName);
            }

            // cannot load
            throw new FileNotFoundException(string.Format(
                "Unable to find localization file: {0}. Fallback files not found.", expectedFileName));
        }

        /// <summary>
        /// 
        /// </summary>
        public void ReloadAllLanguageFiles()
        {
            _strings.Clear();
            _loadedLanguageFiles.ForEach(LoadLanguageFile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
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

        private void LoadLanguageFileImpl(string fileName)
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
                        _strings[propName] = (string)jr.Value;
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