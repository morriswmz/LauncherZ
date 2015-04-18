using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Service;

namespace LauncherZLib.Plugin.Template
{
    // todo: perhaps mixin instead of inheritance is better?
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
            if (query.Arguments == null || query.Arguments.Count == 0)
                return LauncherQuery.EmptyResult;

            ICommandHandler handler = GetCommandHandler(query.Arguments[0]);
            return handler != null
                ? handler.HandleQuery(query)
                : LauncherQuery.EmptyResult;
        }

        public override PostLaunchAction Launch(LauncherData launcherData)
        {
            ICommandHandler handler = GetCommandHandler(launcherData);
            return handler != null
                ? handler.HandleLaunch(launcherData, ((CommandLauncherData) launcherData).CommandArgs)
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
        }

        protected virtual void RemoveCommandHandlers()
        {
            Handlers.Clear();
        }

        protected virtual ICommandHandler GetCommandHandler(LauncherData data)
        {
            var cmdData = data as CommandLauncherData;
            if (cmdData == null || cmdData.CommandArgs == null || cmdData.CommandArgs.Count == 0)
                return null;
            return GetCommandHandler(cmdData.CommandArgs[0]);
        }

        protected virtual ICommandHandler GetCommandHandler(string cmd)
        {
            ICommandHandler handler;
            return Handlers.TryGetValue(cmd, out handler) ? handler : null;
        }
    }
}
