using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.Event
{
    /// <summary>
    /// Marks current method as event handler.
    /// </summary>
    /// <example>
    /// Create a class implementing all event handlers:
    /// <code>
    ///     public class MyEventHandlers {
    ///         ...
    ///         [SubscribeEvent]
    ///         public void HandleTextUpdate(TextUpdateEvent e) {
    ///             ...
    ///         }
    ///         ...
    ///     }
    /// </code>
    /// And register it to an EventBus:
    /// <code>
    ///     myEventBus.register(new MyEventHandlers());
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class SubscribeEventAttribute : Attribute
    {
    }
}
