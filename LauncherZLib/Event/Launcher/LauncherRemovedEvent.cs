using LauncherZLib.Launcher;

namespace LauncherZLib.Event.Launcher
{
    /// <summary>
    /// Occurs when an item of this plugin is removed from the results.
    /// </summary>
    public class LauncherRemovedEvent : LauncherEvent
    {
        public LauncherRemovedEvent(LauncherData launcherData)
            : base(launcherData)
        {
        }
    }
}
