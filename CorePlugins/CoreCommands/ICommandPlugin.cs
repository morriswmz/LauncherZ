using LauncherZLib.Event;
using LauncherZLib.I18N;
using LauncherZLib.Plugin.Service;

namespace CorePlugins.CoreCommands
{
    public interface ICommandPlugin
    {
        IPluginServiceProvider PluginServiceProvider { get; }

        IPluginInfoProvider PluginInfo { get; }

        ILocalizationDictionary Localization { get; }

        IEventBus EventBus { get; }
    }
}
