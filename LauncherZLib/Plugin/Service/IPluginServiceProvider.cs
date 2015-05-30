namespace LauncherZLib.Plugin.Service
{
    public interface IPluginServiceProvider : IExtendedServiceProvider
    {
        /// <summary>
        /// Gets a collections of essential services. You may get other services via
        /// GetService().
        /// </summary>
        /// <remarks>
        /// When plugin is active, essential services should be always available.
        /// </remarks>
        EssentialPluginServices Essentials { get; }
    }
}
