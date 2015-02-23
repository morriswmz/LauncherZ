using LauncherZLib.Icon;

namespace LauncherZLib.Event.Plugin
{
    /// <summary>
    /// <para>Occurs when plugin should register its icons.</para>
    /// <para>If you want to register your own icons, you can do it here.</para>
    /// </summary>
    public sealed class PluginIconRegisterationEvent : EventBase
    {
        private readonly IIconRegisterer _iconRegisterer;

        /// <summary>
        /// Retrieves the icon register of LauncherZApp.
        /// Use it to register your icons.
        /// </summary>
        public IIconRegisterer IconRegisterer { get { return _iconRegisterer; } }

        public PluginIconRegisterationEvent(IIconRegisterer iconRegisterer)
        {
            _iconRegisterer = iconRegisterer;
        }
    }
}
