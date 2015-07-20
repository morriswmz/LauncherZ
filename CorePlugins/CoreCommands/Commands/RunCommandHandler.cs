using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LauncherZ.Icon;
using LauncherZLib.FormattedText;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Modules;
using LauncherZLib.Plugin.Service;

namespace CorePlugins.CoreCommands.Commands
{
    public class RunCommandHandler : CoreCommandHandler
    {
        public RunCommandHandler(IExtendedServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string CommandName
        {
            get { return "run"; }
        }

        public override IEnumerable<LauncherData> HandleQuery(LauncherQuery query)
        {
            if (query.InputArguments.Count <= 1)
                return LauncherQuery.EmptyResult;
            return new[]
            {
                new LauncherData(1.0)
                {
                    Title = string.Format(Localization["RunCommandTitle"], FormattedTextEngine.Escape(string.Join(" ", query.InputArguments.Skip(1)))),
                    Description = Localization["RunCommandNormalDescription"],
                    IconLocation = LauncherZIconSet.Program.ToString()
                }
            };
        }

        public override PostLaunchAction HandleLaunch(LauncherData data, LaunchContext context)
        {
            if (data.UserData == "error")
                return PostLaunchAction.DoNothing;
            try
            {
                ArgumentCollection cmdArgs = context.CurrentQuery.InputArguments;
                string args = cmdArgs.Count > 2 ? string.Join(" ", cmdArgs.Skip(2)) : "";
                Process.Start(cmdArgs[1], args);
                return PostLaunchAction.Default;
            }
            catch
            {
                data.UserData = "error";
                data.Description = Localization["RunCommandErrorDescription"];
                return PostLaunchAction.DoNothing;
            }
        }
    }
}
