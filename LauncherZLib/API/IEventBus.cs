using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.Event;

namespace LauncherZLib.API
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
        /// Resets the event bus.
        /// </summary>
        void UnregisterAll();

        /// <summary>
        /// Posts an event.
        /// </summary>
        /// <param name="e"></param>
        void Post(EventBase e);
    }
}
