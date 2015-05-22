namespace LauncherZLib.Utils
{
    public interface ILogger
    {
        bool IsRunning { get; }

        void Log(string msg, params object[] objects);

        void Fine(string msg, params object[] objects);

        void Info(string msg, params object[] objects);

        void Warning(string msg, params object[] objects);

        void Error(string msg, params object[] objects);

        void Severe(string msg, params object[] objects);
    }
}
