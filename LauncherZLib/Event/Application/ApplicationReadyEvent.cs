namespace LauncherZLib.Event.Application
{
    /// <summary>
    /// Occurs when application is ready for interaction (all resources, plugins are loaded).
    /// </summary>
    public sealed class ApplicationReadyEvent : ApplicationEvent
    {
        /// <summary>
        /// Indicates whether last application termination was caused by a crash.
        /// </summary>
        public bool CrashedLastTime { get; private set; }

        public ApplicationReadyEvent(bool crashedLastTime)
        {
            CrashedLastTime = crashedLastTime;
        }

    }

}
