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

namespace CorePlugins.UnitConverter
{
    [Plugin("LZUnitConverter", FriendlyName = "LauncherZ Unit Converter", Authors = "morriswmz", Version = "1.0.0.0")]
    [Description("Provides convenient conversions between different units.")]
    public class UnitConverterPlugin : EmptyPlugin
    {

        // pattern: {(123[.123]|.123)[e[+/-]123]}{S1}>{S2}
        private static readonly Regex ConversionPattern = new Regex(@"^((?:\d*\.\d+|\d+)(?:e[+\-]?\d+)?)([^>]+)>(.+?)$", RegexOptions.IgnoreCase);
        private static readonly Regex InvalidCharPattern = new Regex(@"[\s\.]");

        private ConversionSystem _conversionSystem;
 

        public override void Activate(IPluginServiceProvider serviceProvider)
        {
            base.Activate(serviceProvider);
            _conversionSystem = new ConversionSystem(Logger);
            string defaultDefFilePath = Path.Combine(PluginInfo.PluginSourceDirectory,
                @"Data\UnitConversionDefinitions.json");
            string defaultUnitNamePath = Path.Combine(PluginInfo.PluginSourceDirectory,
                @"I18N\UnitNames.json");
            _conversionSystem.LoadDefinitionsFrom(defaultDefFilePath);
            _conversionSystem.LoadAliasesFromLauguageFile(
                LocalizationHelper.AddCultureNameToPath(defaultUnitNamePath, Localization.CurrentCulture));
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
                    double result = quantity*c.Factor;
                    // determine localized unit names
                    string fromP = c.FullFromUnitName + ".plural";
                    string toP = c.FullToUnitName + ".plural";
                    string localizedFrom = Math.Abs(quantity - 1.0) < double.Epsilon
                        ? Localization[c.FullFromUnitName]
                        : (Localization.CanTranslate(fromP) ? Localization[fromP] : Localization[c.FullFromUnitName]);
                    string localizedTo = Math.Abs(result - 1.0) < double.Epsilon
                        ? Localization[c.FullToUnitName]
                        : (Localization.CanTranslate(toP) ? Localization[toP] : Localization[c.FullToUnitName]);
                    // format result
                    bool isApprox = Math.Abs(Math.Log10(Math.Abs(c.Factor))) > 4 ||
                                    Math.Abs(Math.Log10(Math.Abs(quantity))) > 4;
                    string title = string.Format("{0} {1} {2} {3} {4}",
                        quantity, localizedFrom,
                        isApprox ? "≈" : "=",
                        quantity*c.Factor, localizedTo);
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
    }
}
