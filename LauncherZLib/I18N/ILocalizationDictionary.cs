using System.Globalization;

namespace LauncherZLib.I18N
{
    public interface ILocalizationDictionary
    {
        /// <summary>
        /// Gets the translated string.
        /// </summary>
        /// <param name="strName"></param>
        /// <returns>Translated string, or original string when fails.</returns>
        string this[string strName] { get; }

        /// <summary>
        /// Gets or sets the current culture of this localization dictionary.
        /// Language files will be reloaded accordingly.
        /// </summary>
        CultureInfo CurrentCulture { get; set; }

        /// <summary>
        /// Translates given string. Provides the same functionality as the string indexer.
        /// </summary>
        /// <param name="strName"></param>
        /// <returns>Translated string, or original string when fails.</returns>
        string Translate(string strName);

        /// <summary>
        /// Checks if given string can be translated.
        /// </summary>
        /// <param name="strName"></param>
        /// <returns></returns>
        bool CanTranslate(string strName);

        /// <summary>
        /// Loads specific language file with fallback.
        /// </summary>
        /// <param name="fileName">Path to the language file. Language suffix should be omitted.</param>
        void LoadLanguageFile(string fileName);

        /// <summary>
        /// Loads specific language file.
        /// </summary>
        /// <param name="fileName">Path to the language file. Language suffix should be omitted.</param>
        /// <param name="fallback">If true, will attempt to load language file for fallback language.</param>
        void LoadLanguageFile(string fileName, bool fallback);

        /// <summary>
        /// Reloads all loaded language files.
        /// </summary>
        void ReloadAllLanguageFiles();
    }
}
