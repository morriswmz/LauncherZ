using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.API;
using LauncherZLib.Launcher;

namespace CorePlugins.AppLauncher
{
    public class AppLauncherPlugin : IPlugin
    {



        public void Activate(IPluginContext pluginContext)
        {
            
        }

        public void Deactivate(IPluginContext pluginContext)
        {
            
        }

        public IEnumerable<LauncherData> Query(LauncherQuery query)
        {
            return Enumerable.Empty<LauncherData>();
        }



    }
}
