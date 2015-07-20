using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using LauncherZ.Icon;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Modules;
using LauncherZLib.Plugin.Service;

namespace CorePlugins.CoreCommands.Commands
{
    public sealed class IpCommandHandler : CoreCommandHandler
    {

        public IpCommandHandler(IExtendedServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string CommandName
        {
            get { return "IP"; }
        }

        public override IEnumerable<LauncherData> HandleQuery(LauncherQuery query)
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                var sb = new StringBuilder();
                foreach (var uAddr in ni.GetIPProperties().UnicastAddresses)
                {
                    sb.AppendLine(uAddr.Address.ToString());
                }
                string description = sb.ToString();
                yield return new LauncherData(1.0)
                {
                    Title = ni.Name,
                    Description = description,
                    IconLocation = LauncherZIconSet.Network.ToString(),
                    UserData = description
                };
            }
        }

        public override PostLaunchAction HandleLaunch(LauncherData data, LaunchContext context)
        {
            Clipboard.SetText(string.Format("{0}{1}{2}", data.Title, Environment.NewLine,
                data.UserData));
            return PostLaunchAction.Default;
        }

        public override void HandleSelection(LauncherData cmdData)
        {
            cmdData.Description = cmdData.UserData + Localization["IpCommandLaunchHint"];
        }

        public override void HandleDeselection(LauncherData cmdData)
        {
            cmdData.Description = cmdData.UserData;
        }

    }
}
