namespace LauncherZLib.Utils
{
    /// <summary>
    /// Describes a logger provider.
    /// </summary>
    public interface ILoggerProvider
    {
        /// <summary>
        /// Creates a logger with specific category.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        ILogger CreateLogger(string category);
    }
}
