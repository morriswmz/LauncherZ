using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.Event
{
    /// <summary>
    /// Basic event definition.
    /// </summary>
    public abstract class EventBase
    {
        protected bool DefaultPrevented;

        /// <summary>
        /// Gets whether the default behavior associated with this event is stopped from happening.
        /// </summary>
        public virtual bool IsDefaultPrevented { get { return DefaultPrevented; } }

        /// <summary>
        /// Stops the default behavior from happending
        /// </summary>
        public virtual void PreventDefault()
        {
            DefaultPrevented = true;
        }

    }
}
