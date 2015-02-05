using LauncherZLib.Launcher;

namespace LauncherZLib.Event.Launcher
{
    /// <summary>
    /// Occurs when an item of this plugin is selected.
    /// </summary>
    public class LauncherSelectedEvent : LauncherEvent
    {
        public LauncherSelectedEvent(LauncherData launcherData)
            : base(launcherData)
        {
        }
    }
}
