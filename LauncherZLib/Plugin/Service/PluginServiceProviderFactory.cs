using System;
using System.Collections.Generic;

namespace LauncherZLib.Plugin.Service
{
    /// <summary>
    /// Creates plugin service providers.
    /// </summary>
    public class PluginServiceProviderFactory
    {
        private static readonly Action<PluginServiceProvider> EmptyInitializer = init => { };

        private readonly Action<PluginServiceProvider> _commonServiceInitializer;
        
        public PluginServiceProviderFactory()
            : this(EmptyInitializer)
        {
        }

        public PluginServiceProviderFactory(Action<PluginServiceProvider> commonServiceInitializer)
        {
            _commonServiceInitializer = commonServiceInitializer;
        }

        public PluginServiceProvider Create(Action<PluginServiceProvider> serviceInitializer)
        {
            var sp = new PluginServiceProvider();
            _commonServiceInitializer.Invoke(sp);
            serviceInitializer.Invoke(sp);
            return sp;
        }

        public PluginServiceProvider Create()
        {
            return Create(EmptyInitializer);
        }

    }
}
