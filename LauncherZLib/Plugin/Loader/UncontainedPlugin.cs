namespace LauncherZLib.Plugin.Loader
{
    public sealed class UncontainedPlugin
    {
        private readonly IPlugin _instance;
        private readonly PluginDiscoveryInfo _info;

        public UncontainedPlugin(IPlugin instance, PluginDiscoveryInfo info)
        {
            _instance = instance;
            _info = info;
        }

        public IPlugin Instance { get { return _instance; } }

        public PluginDiscoveryInfo Info { get { return _info; } }

    }
}
