using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.Launcher;

namespace LauncherZLib.Event
{
    public abstract class LauncherEvent : EventBase
    {
        private readonly LauncherData _launcherData;

        public LauncherData LauncherData { get { return _launcherData; } }

        protected LauncherEvent(LauncherData launcherData)
        {
            _launcherData = launcherData;
        }

        /// <summary>
        /// This event's default behavior cannot be prevented.
        /// Attempting to call this method will result in NotSupportedException.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">
        /// Thrown upon calling this method.
        /// </exception>
        public override void PreventDefault()
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Occurs when a new item of this plugin is added to the results.
    /// </summary>
    public class LauncherAddedEvent : LauncherEvent
    {
        public LauncherAddedEvent(LauncherData launcherData) : base(launcherData)
        {

        }
    }

    /// <summary>
    /// Occurs when an item of this plugin is removed from the results.
    /// </summary>
    public class LauncherRemovedEvent : LauncherEvent
    {
        public LauncherRemovedEvent(LauncherData launcherData) : base(launcherData)
        {
        }
    }

    /// <summary>
    /// Occurs when an item of this plugin is selected.
    /// </summary>
    public class LauncherSelectedEvent : LauncherEvent
    {
        public LauncherSelectedEvent(LauncherData launcherData) : base(launcherData)
        {
        }
    }

    /// <summary>
    /// Occurs when an item of this plugin is deselected.
    /// </summary>
    public class LauncherDeselectedEvent : LauncherEvent
    {
        public LauncherDeselectedEvent(LauncherData launcherData) : base(launcherData)
        {
        }
    }

    /// <summary>
    /// Occurs when an item of this plugin ticks.
    /// A <see cref="T:LauncherZLib.Launcher.LauncherData"/> must be tickable to trigger this event.
    /// See <see cref="P:LauncherZLib.Launcher.LauncherExtendedProperties.Tickable"/> for details.
    /// </summary>
    public class LauncherTickEvent : LauncherEvent {
        
        public LauncherTickEvent(LauncherData launcherData) : base(launcherData)
        {

        }

    }

    /// <summary>
    /// Occurs when user pressed enter on an item of this plugin.
    /// </summary>
    public class LauncherExecutedEvent : LauncherEvent
    {

        private readonly LauncherQuery _currentQuery;

        /// <summary>
        /// Retrieves the current query.
        /// </summary>
        public LauncherQuery CurrentQuery { get { return _currentQuery; } }

        /// <summary>
        /// Gets or sets the modified input after execution.
        /// Set to null or empty to clear current results.
        /// Effective only when IsDefaultPreveted is true.
        /// </summary>
        public string ModifiedInput { get; set; }
        
        /// <summary>
        /// Gets or sets whether application window should be hidden after execution.
        /// Effective only when IsDefaultPreveted is true.
        /// </summary>
        public bool HideWindow { get; set; }

        public LauncherExecutedEvent(LauncherData launcherData, LauncherQuery currentQuery) : base(launcherData)
        {
            _currentQuery = currentQuery;
        }

        /// <summary>
        /// Prevents the default behavior: clear and hide.
        /// ModifiedInput and HideWindow property are effective only after calling this method.
        /// </summary>
        public override void PreventDefault()
        {
            DefaultPrevented = true;
        }
    }

}
