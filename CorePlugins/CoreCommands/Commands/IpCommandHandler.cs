using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using LauncherZLib.Event;
using LauncherZLib.Event.Launcher;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Plugin.Template;

namespace CorePlugins.CoreCommands.Commands
{
    public sealed class IpCommandHandler : CoreCommandHandler
    {

        public IpCommandHandler(IPluginServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string CommandName
        {
            get { return "IP"; }
        }

        public override bool SubscribeToEvents
        {
            get { return false; }
        }

        public override IEnumerable<LauncherData> HandleQuery(LauncherQuery query)
        {
            var results = new List<LauncherData>();
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                var sb = new StringBuilder();
                foreach (var uAddr in ni.GetIPProperties().UnicastAddresses)
                {
                    sb.AppendLine(uAddr.Address.ToString());
                }
                string description = sb.ToString();
                results.Add(new CommandLauncherData(query.Arguments, 1.0)
                {
                    Title = ni.Name,
                    Description = description,
                    IconLocation = "LauncherZ://IconNetwork",
                    StringData = description
                });
            }
            return results;
        }

        public override PostLaunchAction HandleLaunch(LauncherData launcherData, ArgumentCollection arguments)
        {
            Clipboard.SetText(string.Format("{0}{1}{2}", launcherData.Title, Environment.NewLine,
                launcherData.StringData));
            return PostLaunchAction.Default;
        }

        [SubscribeEvent]
        public void LauncherSelectedEventHandler(LauncherSelectedEvent e)
        {
            e.LauncherData.Description = e.LauncherData.StringData + Localization["IpCommandLaunchHint"];
        }

        [SubscribeEvent]
        public void LauncherDeselectedEventHandler(LauncherDeselectedEvent e)
        {
            e.LauncherData.Description = e.LauncherData.StringData;
        }

        
    }
}
