using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.Launcher;

namespace CorePlugins.AppLauncher
{
    public class AppLauncherExtendedProperties : LauncherExtendedProperties
    {

        public string LinkFileLocation { get; private set; }

        public AppLauncherExtendedProperties(string linkFileLocation) : base(false)
        {
            LinkFileLocation = linkFileLocation;
        }

    }
}
