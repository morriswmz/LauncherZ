using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LauncherZLib.API;
using LauncherZLib.Event;
using LauncherZLib.Launcher;

namespace CorePlugins.CoreCommands
{
    public class CoreCommandsPlugin : IPlugin
    {
        private IPluginContext _pluginContext;
        private readonly Dictionary<string, ICommandHandler> _handlers = new Dictionary<string, ICommandHandler>(StringComparer.OrdinalIgnoreCase); 

        public void Activate(IPluginContext pluginContext)
        {
            _pluginContext = pluginContext;
            _pluginContext.EventBus.Register(this);
            AddHandler(new CpuCommandHandler());
        }

        public void Deactivate(IPluginContext pluginContext)
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
            ICommandHandler handler;
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
                ICommandHandler handler;
                if (_handlers.TryGetValue(ccProp.Arguments[0], out handler))
                {
                    handler.HandleTick(e);
                }
            }
        }

        private void AddHandler(ICommandHandler handler)
        {
            _handlers.Add(handler.CommandName, handler);
        }

    }
}
