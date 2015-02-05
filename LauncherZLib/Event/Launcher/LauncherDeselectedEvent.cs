using LauncherZLib.Launcher;

namespace LauncherZLib.Event.Launcher
{
    /// <summary>
    /// Occurs when an item of this plugin is deselected.
    /// </summary>
    public class LauncherDeselectedEvent : LauncherEvent
    {
        public LauncherDeselectedEvent(LauncherData launcherData)
            : base(launcherData)
        {
        }
    }
}
