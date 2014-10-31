using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.API;

namespace LauncherZ.Plugin
{
    interface IPlugin
    {

        IEnumerable<ITaskProvider> GetTaskProviders();

    }
}
