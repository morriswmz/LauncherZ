using System;
using LauncherZLib.Launcher;

namespace LauncherZLib.Event.Launcher
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
        /// The default behavior of this event cannot be prevented.
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
}
