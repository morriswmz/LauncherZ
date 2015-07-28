using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using LauncherZ.Icon;
using LauncherZLib.I18N;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin;
using LauncherZLib.Plugin.Modules;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Utils;

namespace CorePlugins.UnitConverter
{
    [Plugin("LZUnitConverter", FriendlyName = "LauncherZ Unit Converter", Authors = "morriswmz", Version = "1.0.0.0")]
    [Description("Provides convenient conversions between different units.")]
    public class UnitConverterPlugin : EmptyPlugin
    {

        // pattern: {(123[.123]|.123)[e[+/-]123]}{S1}>{S2}
        private static readonly Regex ConversionPattern = new Regex(@"^((?:\d*\.\d+|\d+)(?:e[+\-]?\d+)?)([^>]+)>(.+?)$", RegexOptions.IgnoreCase);

        private UnitInformationRegistry _unitInformationRegistry;
        private ConversionSystem _conversionSystem;

        public override void Activate(IPluginServiceProvider serviceProvider)
        {
            base.Activate(serviceProvider);
            
            string defaultDefFilePath = Path.Combine(PluginInfo.PluginSourceDirectory,
                @"Data\UnitConversionDefinitions.json");
            string defaultUnitNamePath = Path.Combine(PluginInfo.PluginSourceDirectory,
                @"I18N\UnitNames.json");

            var defFileLoader = new DefinitionFileLoader(new[] {defaultDefFilePath}, Logger);
            defFileLoader.LoadAll();
            _unitInformationRegistry = defFileLoader.GetLoadedUnitInformation();
            LoadAliasesFromLauguageFile(
                LocalizationHelper.AddCultureNameToPath(defaultUnitNamePath, Localization.CurrentCulture));
            _conversionSystem = new ConversionSystem(_unitInformationRegistry, defFileLoader.GetLoadedConversionProviders());

            Localization.LoadLanguageFile(defaultUnitNamePath);
        }

        public override void Deactivate(IPluginServiceProvider serviceProvider)
        {
            base.Deactivate(serviceProvider);
            _conversionSystem = null;
        }

        public override IEnumerable<LauncherData> Query(LauncherQuery query)
        {
            // check pattern
            Match m = ConversionPattern.Match(query.OriginalInput.Trim());
            if (!m.Success)
            {
                return LauncherQuery.EmptyResult;
            }
            string fromName = m.Groups[2].Value.Trim();
            string toName = m.Groups[3].Value.Trim();
            // read and check quantity
            double quantity;
            if (!double.TryParse(m.Groups[1].Value, out quantity))
            {
                return LauncherQuery.EmptyResult;
            }
            // check conversions
            return _conversionSystem.GetAllConversions(fromName, toName)
                .Select(c =>
                {
                    double result = c.Converter(quantity);
                    // determine localized unit names
                    string fromPlural = c.From.FullName + ".plural";
                    string toPlural = c.To.FullName + ".plural";
                    string localizedFrom = Math.Abs(quantity - 1.0) < double.Epsilon
                        ? Localization[c.From.FullName]
                        : (Localization.CanTranslate(fromPlural) ? Localization[fromPlural] : Localization[c.From.FullName]);
                    string fromAbbr;
                    if (_unitInformationRegistry.TryGetAbbreviation(c.From, out fromAbbr))
                    {
                        localizedFrom = string.Format("{0} {1}{2}{3}",
                            localizedFrom, Localization["LeftParenthesis"],
                            fromAbbr, Localization["RightParenthesis"]);
                    }
                    string localizedTo = Math.Abs(result - 1.0) < double.Epsilon
                        ? Localization[c.To.FullName]
                        : (Localization.CanTranslate(toPlural) ? Localization[toPlural] : Localization[c.To.FullName]);
                    string toAbbr;
                    if (_unitInformationRegistry.TryGetAbbreviation(c.To, out toAbbr))
                    {
                        localizedTo = string.Format("{0} {1}{2}{3}",
                            localizedTo, Localization["LeftParenthesis"],
                            toAbbr, Localization["RightParenthesis"]);
                    }// format result
                    bool isApprox = Math.Abs(Math.Log10(Math.Abs(result/quantity))) > 4;
                    isApprox = isApprox || Math.Abs(Math.Log10(Math.Abs(quantity))) > 4;
                    string title = string.Format("{0:e2} {1} {2} {3:e2} {4}",
                        quantity, localizedFrom,
                        isApprox ? "≈" : "=",
                        result, localizedTo);
                    return new LauncherData(1.0)
                    {
                        Title = title,
                        Description = Localization["DefaultDescription"],
                        IconLocation = LauncherZIconSet.Calculator.ToString(),
                        UserData = result.ToString(CultureInfo.InvariantCulture)
                    };
                });
        }

        public override PostLaunchAction Launch(LauncherData launcherData, LaunchContext context)
        {
            Clipboard.SetText(launcherData.UserData);
            return PostLaunchAction.DoNothing;
        }

        private void LoadAliasesFromLauguageFile(string path)
        {
            Dictionary<string, string> locDict;
            try
            {
                locDict = JsonUtils.StreamDeserialize<Dictionary<string, string>>(path);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to load aliases from language file \"{0}\". Details:{1}{2}",
                    path, Environment.NewLine, ex);
                return;
            }
            foreach (var pair in locDict)
            {
                if (string.IsNullOrWhiteSpace(pair.Value))
                {
                    continue;
                }
                // strip off "plural" affix
                string fullUnitName = pair.Key.EndsWith(".plural")
                    ? pair.Key.Substring(0, pair.Key.LastIndexOf('.'))
                    : pair.Key;
                if (Unit.IsValidFullName(fullUnitName))
                {
                    _unitInformationRegistry.RegisterAlias(Unit.FromFullName(fullUnitName), pair.Value);
                }
            }
        }

    }
}
