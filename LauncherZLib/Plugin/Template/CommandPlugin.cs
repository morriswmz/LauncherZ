using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Service;

namespace LauncherZLib.Plugin.Template
{

    public abstract class CommandPlugin<TC> : ConfigurablePlugin<TC> where TC : class
    {

        protected Regex CommandNameRegex = new Regex(@"^[\w+_]+$");

        protected Dictionary<string, ICommandHandler> Handlers =
            new Dictionary<string, ICommandHandler>(StringComparer.OrdinalIgnoreCase);

        public override void Activate(IPluginServiceProvider serviceProvider)
        {
            base.Activate(serviceProvider);
            AddCommandHandlers();
        }

        public override void Deactivate(IPluginServiceProvider serviceProvider)
        {
            RemoveCommandHandlers();
            base.Deactivate(serviceProvider);
        }

        public override IEnumerable<LauncherData> Query(LauncherQuery query)
        {
            ICommandHandler handler;
            return Handlers.TryGetValue(query.Arguments[0], out handler)
                ? handler.HandleQuery(query)
                : Enumerable.Empty<LauncherData>();
        }

        public override PostLaunchAction Launch(LauncherData launcherData)
        {
            var extendedProps = launcherData.ExtendedProperties as CommandExtendedProperties;
            if (extendedProps == null || extendedProps.CommandArgs.Count == 0)
                return PostLaunchAction.Default;

            ICommandHandler handler;
            return Handlers.TryGetValue(extendedProps.CommandArgs[0], out handler)
                ? handler.HandleLaunch(launcherData)
                : PostLaunchAction.Default;
        }

        protected abstract void AddCommandHandlers();

        protected virtual void AddCommandHandler(ICommandHandler handler)
        {
            if (!CommandNameRegex.IsMatch(handler.CommandName))
            {
                Logger.Warning("");
                return;
            }
            if (Handlers.ContainsKey(handler.CommandName))
            {
                Logger.Warning("");
            }
            Handlers[handler.CommandName] = handler;
            if (handler.SubscribeToEvents)
            {
                EventBus.Register(handler);
            }
        }

        protected virtual void RemoveCommandHandlers()
        {
            foreach (var handler in Handlers.Values.Where(handler => handler.SubscribeToEvents))
            {
                EventBus.Unregister(handler);
            }
            Handlers.Clear();
        }
    }
}
