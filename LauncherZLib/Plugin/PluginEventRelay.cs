using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.API;
using LauncherZLib.Event;

namespace LauncherZLib.Plugin
{
    public class PluginEventRelay
    {

        private readonly IEventBus _parentEventBus;
        private readonly IEventBus _childEventBus;
        private readonly string _pluginId;

        public PluginEventRelay(string pluginId, IEventBus parent, IEventBus child)
        {
            if (_pluginId == null)
                throw new ArgumentNullException("pluginId");
            if (parent == null)
                throw new ArgumentNullException("parent");
            if (child == null)
                throw new ArgumentNullException("child");
            if (parent == child)
                throw new ArgumentException("Parent event bus and child event bus should be distinct.");

            _parentEventBus = parent;
            _childEventBus = child;
            child.Register(this);
        }

        public void Unlink()
        {
            _childEventBus.Unregister(this);
        }

        [SubscribeEvent]
        public void LauncherResultUpdateHandler(LauncherResultUpdateEvent e)
        {
            _parentEventBus.Post(new LauncherResultUpdateEventIntl(_pluginId, e));
        }

    }
}
