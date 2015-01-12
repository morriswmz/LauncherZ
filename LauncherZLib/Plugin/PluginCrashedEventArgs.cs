using System;

namespace LauncherZLib.Plugin
{
    public class PluginCrashedEventArgs : PluginManagerEventArgs
    {
        public string FriendlyMessage { get; private set; }

        public PluginCrashedEventArgs(string pluginId, string friendlyMsg) : base(pluginId)
        {
            FriendlyMessage = friendlyMsg;
        }
    }
}