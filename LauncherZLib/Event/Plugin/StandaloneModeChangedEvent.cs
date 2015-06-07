namespace LauncherZLib.Event.Plugin
{
    /// <summary>
    /// Occurs when standalone mode is enabled/disabled for a plugin.
    /// </summary>
    public sealed class StandaloneModeChangedEvent : EventBase
    {
        /// <summary>
        /// Gets whether standalone mode is enabled after the change.
        /// </summary>
        public bool IsStandaloneModeEnabled { get; private set; }

        public StandaloneModeChangedEvent(bool isStandaloneModeEnabled)
        {
            IsStandaloneModeEnabled = isStandaloneModeEnabled;
        }
        
    }
}
