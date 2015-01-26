using System;
using System.Collections.Generic;
using System.IO;
using LauncherZLib.API;
using LauncherZLib.Launcher;

namespace LauncherZLib.PluginTemplate
{
    public abstract class EmptyPlugin : IPlugin
    {
        protected IPluginContext Context;

        public virtual void Activate(IPluginContext pluginContext)
        {
            // setup context
            Context = pluginContext;
            // prepare working directory
            if (!Directory.Exists(Context.SuggestedDataDirectory))
            {
                try
                {
                    Directory.CreateDirectory(Context.SuggestedDataDirectory);
                }
                catch (Exception ex)
                {
                    Context.Logger.Error(string.Format(
                        "An exception occurred while creating data directory:{0}{1}",
                        Environment.NewLine, ex));
                }
            }
            
                
        }

        public abstract void Deactivate(IPluginContext pluginContext);

        public abstract IEnumerable<LauncherData> Query(LauncherQuery query);
    }
}
