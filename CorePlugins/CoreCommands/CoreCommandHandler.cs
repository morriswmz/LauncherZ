﻿using System;
using System.Collections.Generic;
using LauncherZLib.Event.Launcher;
using LauncherZLib.I18N;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Plugin.Template;

namespace CorePlugins.CoreCommands
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

        public abstract IEnumerable<LauncherData> HandleQuery(LauncherQuery query);

        public abstract PostLaunchAction HandleLaunch(LauncherData launcherData);

        public abstract bool SubscribeToEvents { get; }

    }
}
