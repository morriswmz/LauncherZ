using System;
using System.Collections.Generic;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin;

namespace LauncherZLib.PluginTemplate
{
    public abstract class CommandPlugin<T> : ConfigurablePlugin<T> where T : class
    {

        protected Dictionary<string, ICommandHandler> Handlers;

        public override void Activate(IPluginContext pluginContext)
        {
            base.Activate(pluginContext);
            AddCommandHandlers();
        }

        public override void Deactivate(IPluginContext pluginContext)
        {
            RemoveCommandHandlers();
            base.Deactivate(pluginContext);
        }

        public override IEnumerable<LauncherData> Query(LauncherQuery query)
        {
            throw new NotImplementedException();
        }

        protected abstract void AddCommandHandlers();

        protected virtual void RemoveCommandHandlers()
        {
            
        }

    }
}
