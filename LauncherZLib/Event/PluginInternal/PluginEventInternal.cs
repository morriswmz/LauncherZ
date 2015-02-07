using System;

namespace LauncherZLib.Event.PluginInternal
{
    /// <summary>
    /// <para>
    /// Base class for internal plugin events. Internal usage only. Plugins should use events
    /// under <see cref="N:LauncherZLib.Event.Plugin"/>.
    /// </para>
    /// <para>
    /// When a plugin posts an event, it does not associate its plugin id with the event.
    /// The event relay of the plugin container will tag the original event with correct
    /// plugin id using internal plugin events and post it to the parent event bus (usually
    /// the plugin manager's).
    /// </para>
    /// </summary>
    public abstract class PluginEventInternal<TE> : EventBase
        where TE : EventBase
    {
        /// <summary>
        /// Gets the id of the plugin posting this event.
        /// </summary>
        public string SourceId { get; private set; }

        /// <summary>
        /// Gets the original event.
        /// </summary>
        public TE BaseEvent { get; private set; }

        protected PluginEventInternal(TE baseEvent, string sourceId)
        {
            if (baseEvent == null)
                throw new ArgumentNullException("baseEvent");
            if (string.IsNullOrEmpty(sourceId))
                throw new Exception("Source id cannot be null or empty.");

            BaseEvent = baseEvent;
            SourceId = sourceId;
        }

    }
}
