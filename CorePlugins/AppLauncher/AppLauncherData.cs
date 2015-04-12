using LauncherZLib.Launcher;

namespace CorePlugins.AppLauncher
{
    public class AppLauncherData : LauncherData
    {
        public string LinkFileLocation { get; private set; }

        public AppLauncherData(string linkFileLocation, double relevance) : base(relevance)
        {
            LinkFileLocation = linkFileLocation;
        }

    }
}
