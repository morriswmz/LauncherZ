﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.API;
using LauncherZLib.Launcher;

namespace LauncherZLib.PluginTemplate
{
    public abstract class CommandPlugin<T> : ConfigurablePlugin<T> where T : class
    {

        public override void Activate(IPluginContext pluginContext)
        {
            base.Activate(pluginContext);
        }

        public override IEnumerable<LauncherData> Query(LauncherQuery query)
        {
            throw new NotImplementedException();
        }
    }
}
