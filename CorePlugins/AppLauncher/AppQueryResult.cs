using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.Launcher;
using LauncherZLib.Matching;

namespace CorePlugins.AppLauncher
{
    public sealed class AppQueryResult
    {

        public string Title { get; private set; }

        public string Description { get; private set; }

        public string LinkFileLocation { get; private set; }

        public double Relevance { get; set; }

        public AppQueryResult(AppDescription appDescription, FlexMatchResult result)
        {
            
        }

        public LauncherData CreateLauncherData()
        {
            return new LauncherData(Title, Description, LinkFileLocation, Relevance,
                new AppLauncherExtendedProperties(LinkFileLocation));
        }
    }
}
