using System;
using System.Collections.Concurrent;
using LauncherZLib.Event;
using LauncherZLib.Event.Plugin;
using LauncherZLib.Event.PluginInternal;
using LauncherZLib.Utils;

namespace LauncherZLib.Plugin
{
    /// <summary>
    /// <para>Event pus for plugins.</para>
    /// <para>
    /// In addition to the basic functionalities. This event bus will forward certain event to its
    /// parent event bus.
    /// </para>
    /// </summary>
    public sealed class PluginEventBus : EventBus
    {
        // thread-safe implementation
        private static readonly ConcurrentDictionary<Type, EventRelay> EventRelays =
            new ConcurrentDictionary<Type, EventRelay>();
 
        private readonly IDispatcherService _dispatcherService;
        private readonly IEventBus _parent;
        private readonly string _pluginId;

        static PluginEventBus()
        {
            // register default relays
            RegisterEventRelay(typeof (QueryResultUpdateEvent),
                (s, e) => new QueryResultUpdateEventI(s, (QueryResultUpdateEvent) e), true);
        }

        /// <summary>
        /// Creates an event bus for plugins.
        /// </summary>
        /// <param name="pluginId">Id of the associated plugin.</param>
        /// <param name="parent">Parent event bus.</param>
        /// <param name="dispatcherService">Dispatcher service associated with the parent event bus.</param>
        public PluginEventBus(string pluginId, IEventBus parent, IDispatcherService dispatcherService)
        {
            _dispatcherService = dispatcherService;
            _parent = parent;
            _pluginId = pluginId;
        }

        /// <summary>
        /// Registers a new event relay.
        /// </summary>
        /// <param name="t">Type of the event.</param>
        /// <param name="packager">
        /// A function that packages the original event before posting it one parent event bus.
        /// </param>
        /// <param name="requireSync">
        /// Whether this event should be posted via parent event bus's thread using specified dispatcher.
        /// </param>
        public static void RegisterEventRelay(Type t, Func<string, EventBase, EventBase> packager, bool requireSync)
        {
            if (t == null)
                throw new ArgumentNullException("t");
            if (!typeof (EventBase).IsAssignableFrom(t))
                throw new ArgumentException("t must be a event type.");
            if (packager == null)
                throw new ArgumentNullException("packager");

            EventRelays[t] = new EventRelay()
            {
                Packager = packager,
                RequireSynchronize = requireSync
            };
        }

        public override void Post(EventBase e)
        {
            base.Post(e);
            Type eventType = e.GetType();
            EventRelay relay;
            if (EventRelays.TryGetValue(eventType, out relay))
            {
                if (relay.RequireSynchronize && !_dispatcherService.CheckAccess())
                {
                    EventBase packagedEvent = relay.Packager.Invoke(_pluginId, e);
                    _dispatcherService.InvokeAsync(() => _parent.Post(packagedEvent));
                }
                else
                {
                    _parent.Post(relay.Packager.Invoke(_pluginId, e)); 
                }
            }
        }

        sealed class EventRelay
        {
            public bool RequireSynchronize { get; set; }
            public Func<string, EventBase, EventBase> Packager { get; set; } 
        }

    }
}
