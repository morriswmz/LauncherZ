using System.Collections.Generic;
using LauncherZLib.Plugin;

namespace LauncherZLib.Launcher
{
    /// <summary>
    /// Sorts Launcher data entries.
    /// </summary>
    public sealed class LauncherDataComparer : IComparer<LauncherData>
    {
        private readonly PluginManager _pluginManager;

        public LauncherDataComparer(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;
        }

        public int Compare(LauncherData x, LauncherData y)
        {
            if (x.Relevance > y.Relevance)
                return 1;

            if (x.Relevance < y.Relevance)
                return -1;

            double xp = _pluginManager.GetPluginPriority(x.PluginId);
            double yp = _pluginManager.GetPluginPriority(y.PluginId);
            return xp.CompareTo(yp);
        }
    }
}
