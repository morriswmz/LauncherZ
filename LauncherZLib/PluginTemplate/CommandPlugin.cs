using System;
using System.Collections.Generic;
using LauncherZLib.API;
using LauncherZLib.Launcher;

namespace LauncherZLib.PluginTemplate
{
    public abstract class CommandPlugin<T> : ConfigurablePlugin<T> where T : class
    {

        protected Dictionary<string, ICommandHandler> Handlers;

        public override void Activate(IPluginContext pluginContext)
        {
            base.Activate(pluginContext);
        }

        public override void Deactivate(IPluginContext pluginContext)
        {

            base.Deactivate(pluginContext);
        }

        public override IEnumerable<LauncherData> Query(LauncherQuery query)
        {
            throw new NotImplementedException();
        }

        protected abstract void AddCommandHandlers();

        protected void RemoveCommandHandlers()
        {
            
        }

    }
}
