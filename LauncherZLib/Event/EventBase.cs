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
        private bool _defaultPrevented = false;

        /// <summary>
        /// Gets whether the default behavior associated with this event is stopped from happening.
        /// </summary>
        public bool IsDefaultPrevented { get { return _defaultPrevented; } }

        /// <summary>
        /// Stops the default behavior from happending
        /// </summary>
        public void PreventDefault()
        {
            _defaultPrevented = true;
        }

    }
}
