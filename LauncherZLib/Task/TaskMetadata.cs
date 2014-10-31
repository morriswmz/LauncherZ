namespace LauncherZLib.Task
{

    public class TaskMetadata
    {
        private static long _uidCounter = 0;

        private readonly string _providerId;
        private readonly long _uniqueId;

        public TaskMetadata(string providerId, bool executable, bool tickable)
            : this(providerId, executable, tickable, TabBehavior.None) { }

        public TaskMetadata(string providerId, bool executable, bool tickable, TabBehavior tabBehavior)
        {
            _providerId = providerId;
            Executable = executable;
            Tickable = tickable;
            _uniqueId = _uidCounter++;
            TabBehavior = tabBehavior;
        }

        /// <summary>
        /// Gets the unique id associated with the metadata.
        /// Each instance of TaskMetaData will be assigned a unique id.
        /// </summary>
        public long UniqueId { get { return _uniqueId; } }
        
        public string ProviderId { get { return _providerId; } }

        public bool Executable { get; set; }

        public bool Tickable { get; set; }

        public TabBehavior TabBehavior { get; set; }

    }

    public enum TabBehavior
    {
        None,
        Detail
    }

}
