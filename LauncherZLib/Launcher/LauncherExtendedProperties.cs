using System.Threading;

namespace LauncherZLib.Launcher
{

    /// <summary>
    /// Stores extended properties of a command.
    /// You can extend this class to store your own data.
    /// </summary>
    public class LauncherExtendedProperties
    {
        private static long _uidCounter = 0;

        private readonly long _uniqueId;

        public LauncherExtendedProperties(bool tickable)
            : this(tickable, TickRate.Fast) { }

        public LauncherExtendedProperties(bool tickable, TickRate rate)
        {
            Tickable = tickable;
            CurrentTickRate = rate;
            _uniqueId = Interlocked.Increment(ref _uidCounter);
        }

        /// <summary>
        /// Gets the unique id associated with the metadata.
        /// Each instance of TaskMetaData will be assigned a unique id.
        /// </summary>
        public long UniqueId { get { return _uniqueId; } }

        public bool Tickable { get; set; }

        public TickRate CurrentTickRate { get; set; }

    }

}
