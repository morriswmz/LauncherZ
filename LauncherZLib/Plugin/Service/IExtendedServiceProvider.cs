using System;

namespace LauncherZLib.Plugin.Service
{
    public interface IExtendedServiceProvider : IServiceProvider
    {
        bool CanProvideService<T>() where T : class;

        bool CanProvideService(Type t);

        T GetService<T>() where T : class;
    }
}
