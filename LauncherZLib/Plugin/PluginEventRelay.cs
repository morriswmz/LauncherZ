using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.API;
using LauncherZLib.Event;

namespace LauncherZLib.Plugin
{

    /// <summary>
    /// Passes relevant events raised by plugins to parent event bus,
    /// converting them to tagged version.
    /// </summary>
    public class PluginEventRelay
    {

        private readonly IEventBus _parentEventBus;
        private readonly IEventBus _childEventBus;
        private readonly string _pluginId;

        public PluginEventRelay(string pluginId, IEventBus parent, IEventBus child)
        {
            if (pluginId == null)
                throw new ArgumentNullException("pluginId");
            if (parent == null)
                throw new ArgumentNullException("parent");
            if (child == null)
                throw new ArgumentNullException("child");
            if (parent == child)
                throw new ArgumentException("Parent event bus and child event bus should be distinct.");

            _pluginId = pluginId;
            _parentEventBus = parent;
            _childEventBus = child;
            Link();
        }

        /// <summary>
        /// Gets if the relay is unlinked.
        /// </summary>
        public bool IsLinked { get; private set; }

        /// <summary>
        /// Links the relay to child event bus.
        /// </summary>
        public void Link()
        {
            _childEventBus.Register(this);
            IsLinked = true;
        }

        /// <summary>
        /// Unlinks the relay from child event bus.
        /// </summary>
        public void Unlink()
        {
            _childEventBus.Unregister(this);
            IsLinked = false;
        }

        #region Event Relays

        [SubscribeEvent]
        public void LauncherResultUpdateHandler(LauncherResultUpdateEvent e)
        {
            _parentEventBus.Post(new LauncherResultUpdateEventTagged(_pluginId, e));
        }

        #endregion
    }
}
