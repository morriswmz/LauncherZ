using System;
using System.Collections.Generic;
using LauncherZLib.I18N;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Modules;
using LauncherZLib.Plugin.Service;

namespace CorePlugins.CoreCommands.Commands
{
    public abstract class CoreCommandHandler : ICommandHandler
    {
        protected IExtendedServiceProvider ServiceProvider;
        protected ILocalizationDictionary Localization;

        protected CoreCommandHandler(IExtendedServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            ServiceProvider = serviceProvider;
            Localization = serviceProvider.GetService<ILocalizationDictionary>();
        }

        public abstract string CommandName { get; }

        public abstract IEnumerable<LauncherData> HandleQuery(LauncherQuery query);

        public abstract PostLaunchAction HandleLaunch(LauncherData data, LaunchContext context);

        public virtual void HandleTick(LauncherData cmdData)
        {
            
        }

        public virtual void HandleSelection(LauncherData cmdData)
        {
            
        }

        public virtual void HandleDeselection(LauncherData cmdData)
        {
            
        }

    }
}
