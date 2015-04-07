namespace LauncherZLib.Event
{

    public interface IEventBus
    {
        /// <summary>
        /// Registers an object containing event handling methods.
        /// </summary>
        /// <param name="obj"></param>
        void Register(object obj);

        /// <summary>
        /// Unregister an object, and remove all event handlers in that object.
        /// </summary>
        /// <param name="obj"></param>
        void Unregister(object obj);

        /// <summary>
        /// Posts an event.
        /// </summary>
        /// <param name="e"></param>
        void Post(EventBase e);
    }
}
