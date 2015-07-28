using System;

namespace LauncherZLib.I18N
{
    public class LanguageFilesAttribute : Attribute
    {

        private readonly string[] _languageFiles;

        public LanguageFilesAttribute(params string[] languageFiles)
        {
            _languageFiles = languageFiles ?? new string[0];
        }

        public string[] LanguageFiles
        {
            get
            {
                var result = new string[_languageFiles.Length];
                if (result.Length > 0)
                {
                    Array.Copy(_languageFiles, result, 0);
                }
                return result;
            }
        }

    }
}
