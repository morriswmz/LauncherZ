namespace LauncherZLib.Utils
{
    public interface ILogger
    {
        bool IsRunning { get; }

        void Log(string msg);

        void Fine(string msg);

        void Info(string msg);

        void Warning(string msg);

        void Error(string msg);

        void Severe(string msg);
    }
}
