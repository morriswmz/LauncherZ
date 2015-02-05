using LauncherZLib.Launcher;

namespace LauncherZLib.Event.Launcher
{
    /// <summary>
    /// Occurs when an item of this plugin ticks.
    /// A <see cref="T:LauncherZLib.Launcher.LauncherData"/> must be tickable to trigger this event.
    /// See <see cref="P:LauncherZLib.Launcher.LauncherExtendedProperties.Tickable"/> for details.
    /// </summary>
    public class LauncherTickEvent : LauncherEvent
    {

        public LauncherTickEvent(LauncherData launcherData)
            : base(launcherData)
        {

        }

    }
}
