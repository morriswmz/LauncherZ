namespace LauncherZLib.Event
{

    public interface IEventBus
    {
        /// <summary>
        /// Registers an event handler.
        /// No action will be taken if given event handler is already registered.
        /// </summary>
        /// <param name="obj"></param>
        /// <seealso cref="T:LauncherZLib.Event.SubscribeEventAttribute"/>
        void Register(object obj);

        /// <summary>
        /// Unregisters a specific event handler.
        /// No action will be taken if given event handler is not registered.
        /// </summary>
        /// <param name="obj"></param>
        void Unregister(object obj);

        /// <summary>
        /// Posts an event.
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// This method distributes given event to a cached event handler list. Adding/removing event handlers
        /// while handling the event will not affect the cached event handler list.
        /// Although this class is thread-safe, you should still think carefully in multi-threaded environment.
        /// </remarks>
        void Post(EventBase e);
        
    }
}
