using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using LauncherZ.Icon;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin;
using LauncherZLib.Plugin.Modules;
using LauncherZLib.Plugin.Service;

namespace CorePlugins.MathEvaluator
{
    [Plugin("LZMathEvaluator", FriendlyName = "LauncherZ Math Evaluator", Authors = "morriswmz", Version = "1.0.0.0")]
    [Description("Evaluates math expressions.")]
    public class MathEvaluatorPlugin : EmptyPlugin
    {

        public override void Activate(IPluginServiceProvider serviceProvider)
        {
            base.Activate(serviceProvider);
            Logger.Fine("This is MathEvaluator. Life = 42");
        }

        public override void Deactivate(IPluginServiceProvider serviceProvider)
        {
            // nothing to do here
        }

        public override IEnumerable<LauncherData> Query(LauncherQuery query)
        {
            double val;
            if (MathEvaluator.Instance.TryEvaluate(query.OriginalInput, out val))
            {
                return new[]
                {
                    new LauncherData(1.0)
                    {
                        Title = val.ToString(CultureInfo.InvariantCulture),
                        Description = Localization["MathEvaluatorDescription"],
                        IconLocation = LauncherZIconSet.Calculator.ToString()
                    }
                };
            }
            return LauncherQuery.EmptyResult;
        }

        public override PostLaunchAction Launch(LauncherData launcherData, LaunchContext context)
        {
            Clipboard.SetText(launcherData.Title);
            return PostLaunchAction.DoNothing;
        }

    }
}
