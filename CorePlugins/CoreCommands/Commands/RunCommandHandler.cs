using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Plugin.Template;

namespace CorePlugins.CoreCommands.Commands
{
    public class RunCommandHandler : CoreCommandHandler
    {
        public RunCommandHandler(IPluginServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string CommandName
        {
            get { return "run"; }
        }

        public override IEnumerable<CommandLauncherData> HandleQuery(LauncherQuery query)
        {
            if (query.Arguments.Count <= 1)
                return Enumerable.Empty<CommandLauncherData>();
            return new[]
            {
                new CommandLauncherData(query.Arguments, 1.0)
                {
                    Title = string.Format(Localization["RunCommandTitle"], string.Join(" ", query.Arguments.Skip(1))),
                    Description = Localization["RunCommandNormalDescription"],
                    IconLocation = "LauncherZ://IconProgram"
                }
            };
        }

        public override PostLaunchAction HandleLaunch(CommandLauncherData cmdData)
        {
            if (cmdData.StringData == "error")
                return PostLaunchAction.DoNothing;
            try
            {
                string args = cmdData.CommandArgs.Count > 2 ? string.Join(" ", cmdData.CommandArgs.Skip(2)) : "";
                Process.Start(cmdData.CommandArgs[1], args);
                return PostLaunchAction.Default;
            }
            catch
            {
                cmdData.StringData = "error";
                cmdData.Description = Localization["RunCommandErrorDescription"];
                return PostLaunchAction.DoNothing;
            }
        }
    }
}
