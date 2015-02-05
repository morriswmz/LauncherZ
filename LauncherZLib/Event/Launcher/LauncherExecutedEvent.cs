using LauncherZLib.Launcher;

namespace LauncherZLib.Event.Launcher
{
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

        public LauncherExecutedEvent(LauncherData launcherData, LauncherQuery currentQuery)
            : base(launcherData)
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
