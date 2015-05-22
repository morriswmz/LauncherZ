using LauncherZ.Icon;
using LauncherZLib.Launcher;

namespace CorePlugins.BookmarkLauncher
{
    sealed class BookmarkQueryResult
    {
        public string Title { get; private set; }

        public string Description { get; private set; }

        public string Url { get; private set; }

        public double Relevance { get; private set; }

        public BookmarkQueryResult(string title, string description, string url, double relevance)
        {
            Title = title;
            Description = description;
            Url = url;
            Relevance = relevance;
        }

        public LauncherData ToLauncherData()
        {
            return new LauncherData(Relevance)
            {
                Title = Title,
                Description = Description,
                IconLocation = LauncherZIconSet.Network.ToString(),
                StringData = Url
            };
        }
    }
}
