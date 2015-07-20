namespace LauncherZLib.Launcher
{
    /// <summary>
    /// Provides context information when launching.
    /// </summary>
    public class LaunchContext
    {
        public LaunchContext(LauncherQuery currentQuery)
        {
            CurrentQuery = currentQuery;
        }

        public LauncherQuery CurrentQuery { get; private set; }

    }
}
