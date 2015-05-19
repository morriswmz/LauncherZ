using LauncherZLib.Launcher;

namespace CorePlugins.AppLauncher
{
    public sealed class AppQueryResult
    {

        public string Title { get; private set; }

        public string Description { get; private set; }

        public string LinkFileLocation { get; private set; }

        public double Relevance { get; set; }

        public AppQueryResult(string title, string description, string linkFileLocation, double relevance)
        {
            Title = title;
            Description = description;
            LinkFileLocation = linkFileLocation;
            Relevance = relevance;
        }

        public LauncherData CreateLauncherData()
        {
            // use string data to store link file location
            return new LauncherData(Relevance)
            {
                Title = Title, 
                Description = Description, 
                IconLocation = "file://" + LinkFileLocation,
                StringData = LinkFileLocation
            };
        }
    }
}
