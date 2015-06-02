using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using LauncherZ.Icon;
using LauncherZLib.FormattedText;
using LauncherZLib.I18N;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Plugin.Template;

namespace LauncherZ.Plugin
{
    class PluginCommandHandler : BasicCommandHandler
    {
        public PluginCommandHandler(IPluginServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override string CommandName
        {
            get { return "lz-plugin"; }
        }

        public override IEnumerable<CommandLauncherData> HandleQuery(LauncherQuery query)
        {
            if (query.Arguments.Count == 1)
            {
                PluginManager pm = LauncherZApp.Instance.PluginManager;
                ILocalizationDictionary localization = ServiceProvider.Essentials.Localization;
                return pm.LoadedPluginIds.Where(x => x != ServiceProvider.Essentials.PluginInfo.PluginId)
                    .Select(x =>
                    {
                        PluginContainer pc = pm.GetPluginContainer(x);
                        var title = string.Format(localization["PluginCommandListTitle"],
                            FormattedTextEngine.Escape(pc.PluginFriendlyName),
                            FormattedTextEngine.Escape(string.Join(localization["AuthorSeparator"], pc.PluginAuthors)));
                        var description = string.Format(localization["PluginCommandListDescription"],
                            string.IsNullOrWhiteSpace(pc.PluginDescription)
                                ? localization["NoDescriptionGiven"]
                                : pc.PluginDescription,
                            localization[pm.IsPluginActivated(x) ? "PluginStatusActivated" : "PluginStatusDeactivated"]
                            );
                        return new CommandLauncherData(query.Arguments, 1.0)
                        {
                            Title = title,
                            Description = description,
                            IconLocation = pm.IsPluginActivated(x) ? LauncherZIconSet.ComponentActive.ToString() : LauncherZIconSet.ComponentInactive.ToString()
                        };
                    });
            }
            else
            {
                return Enumerable.Empty<CommandLauncherData>();
            }
        }

        public override PostLaunchAction HandleLaunch(CommandLauncherData cmdData)
        {
            return PostLaunchAction.DoNothing;
        }
    }
}
