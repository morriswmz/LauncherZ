using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using LauncherZLib.Event;
using LauncherZLib.Event.Launcher;
using LauncherZLib.I18N;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin;
using LauncherZLib.Plugin.Service;

namespace CorePlugins.CoreCommands
{
    [Plugin("LZCoreCommands", FriendlyName = "LauncherZ Core Commands", Authors = "morriswmz", Version = "0.1.0.0")]
    [Description("Provides basic commands.")]
    public class CoreCommandsPlugin : IPlugin, ICommandPlugin
    {
        private readonly Dictionary<string, CommandHandler> _handlers = new Dictionary<string, CommandHandler>(StringComparer.OrdinalIgnoreCase);

        public IPluginServiceProvider PluginServiceProvider { get; private set; }

        public IPluginInfoProvider PluginInfo { get; private set; }

        public ILocalizationDictionary Localization { get; private set; }

        public IEventBus EventBus { get; private set; }

        public void Activate(IPluginServiceProvider serviceProvider)
        {
            PluginServiceProvider = serviceProvider;
            PluginInfo = PluginServiceProvider.GetService<IPluginInfoProvider>();
            EventBus = PluginServiceProvider.GetService<IEventBus>();
            EventBus.Register(this);
            Localization = PluginServiceProvider.GetService<ILocalizationDictionary>();
            Localization.LoadLanguageFile(
                Path.Combine(PluginInfo.PluginSourceDirectory, @"I18N\CoreCommandsStrings.json"));
            AddHandler(new CpuCommandHandler(this));
            AddHandler(new ExitCommandHandler(this));
        }

        public void Deactivate(IPluginServiceProvider serviceProvider)
        {
            foreach (var commandHanlder in _handlers.Values)
            {
                var hanlder = commandHanlder as IDisposable;
                if (hanlder != null)
                    hanlder.Dispose();
            }
        }

        public IEnumerable<LauncherData> Query(LauncherQuery query)
        {
            CommandHandler handler;
            if (_handlers.TryGetValue(query.Arguments[0], out handler))
            {
                return handler.HandleQuery(query);
            }
            else
            {
                return Enumerable.Empty<LauncherData>();
            }
        }


        [SubscribeEvent]
        public void LauncherTickHandler(LauncherTickEvent e)
        {
            var ccProp = e.LauncherData.ExtendedProperties as CommandExtendedProperties;
            if (ccProp != null)
            {
                CommandHandler handler;
                if (_handlers.TryGetValue(ccProp.Arguments[0], out handler))
                {
                    handler.HandleTick(e);
                }
            }
        }

        [SubscribeEvent]
        public void LauncherExecutedHandler(LauncherExecutedEvent e)
        {
            var ccProp = e.LauncherData.ExtendedProperties as CommandExtendedProperties;
            if (ccProp != null)
            {
                CommandHandler handler;
                if (_handlers.TryGetValue(ccProp.Arguments[0], out handler))
                {
                    handler.HandleExecute(e);
                }
            }
        }

        private void AddHandler(CommandHandler handler)
        {
            _handlers.Add(handler.CommandName, handler);
        }

        
    }
}
