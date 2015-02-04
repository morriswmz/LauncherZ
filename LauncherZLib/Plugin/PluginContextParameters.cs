using System.Windows.Threading;
using LauncherZLib.API;

namespace LauncherZLib.Plugin
{
    /// <summary>
    /// This class exists to reduce the number of parameters in the constructor of
    /// <see cref="T:LauncherZLib.Plugin.PluginContainer"/>.
    /// </summary>
    public class PluginContextParameters
    {
        public ILogger Logger { get; set; }

        public IDispatcherService DispatcherService { get; set; }

        public IEventBus ParentEventBus { get; set; }

    }
}
