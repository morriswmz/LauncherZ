using LauncherZLib.Launcher;

namespace LauncherZLib.Event.Launcher
{
    /// <summary>
    /// Occurs when a new item of this plugin is added to the results.
    /// </summary>
    public class LauncherAddedEvent : LauncherEvent
    {
        public LauncherAddedEvent(LauncherData launcherData)
            : base(launcherData)
        {

        }
    }
}
