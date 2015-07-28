using System.Collections.Generic;
using System.Linq;
using LauncherZ.Icon;
using LauncherZLib.FormattedText;
using LauncherZLib.I18N;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin;
using LauncherZLib.Plugin.Modules;
using LauncherZLib.Plugin.Service;

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

        public override IEnumerable<LauncherData> HandleQuery(LauncherQuery query)
        {
            if (query.InputArguments.Count == 1)
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
                            string.IsNullOrWhiteSpace(pc.LocalizedPluginDescription)
                                ? localization["NoDescriptionGiven"]
                                : pc.LocalizedPluginDescription,
                            localization[pm.IsPluginActivated(x) ? "PluginStatusActivated" : "PluginStatusDeactivated"]
                            );
                        return new LauncherData(1.0)
                        {
                            Title = title,
                            Description = description,
                            IconLocation = pm.IsPluginActivated(x) ? LauncherZIconSet.ComponentActive.ToString() : LauncherZIconSet.ComponentInactive.ToString()
                        };
                    });
            }
            
            return LauncherQuery.EmptyResult;    
        }

        public override PostLaunchAction HandleLaunch(LauncherData data, LaunchContext context)
        {
            return PostLaunchAction.DoNothing;
        }
    }
}
