using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.Plugin
{
    public enum PluginStatus
    {
        /// <summary>
        /// Plugin is deactivated, can be activated.
        /// </summary>
        Deactivated,
        
        /// <summary>
        /// Plugin is successfully activated.
        /// </summary>
        Activated,
        
        /// <summary>
        /// Plugin has crashed, and cannot be activated/deactivated.
        /// </summary>
        Crashed
    }
}
