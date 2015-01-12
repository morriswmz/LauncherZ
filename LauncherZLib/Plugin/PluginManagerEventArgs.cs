using System;

namespace LauncherZLib.Plugin
{
    public class PluginManagerEventArgs : EventArgs
    {
        
        public PluginManagerEventArgs(string pluginId)
        {
            PluginId = pluginId;
        }

        public string PluginId { get; private set; }
    }
    
}