using System;
using System.Collections.Generic;
using LauncherZLib.I18N;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Plugin.Template;

namespace CorePlugins.CoreCommands.Commands
{
    public abstract class CoreCommandHandler : ICommandHandler
    {
        protected IPluginServiceProvider ServiceProvider;
        protected ILocalizationDictionary Localization;

        protected CoreCommandHandler(IPluginServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            ServiceProvider = serviceProvider;
            Localization = serviceProvider.GetService<ILocalizationDictionary>();
        }

        public abstract string CommandName { get; }

        public abstract IEnumerable<CommandLauncherData> HandleQuery(LauncherQuery query);

        public abstract PostLaunchAction HandleLaunch(CommandLauncherData cmdData);

        public virtual void HandleTick(CommandLauncherData cmdData)
        {
            
        }

        public virtual void HandleSelection(CommandLauncherData cmdData)
        {
            
        }

        public virtual void HandleDeselection(CommandLauncherData cmdData)
        {
            
        }

    }
}
