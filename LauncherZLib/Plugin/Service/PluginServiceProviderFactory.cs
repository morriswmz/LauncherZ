using System;
using System.Collections.Generic;

namespace LauncherZLib.Plugin.Service
{
    /// <summary>
    /// Creates plugin service providers.
    /// </summary>
    public class PluginServiceProviderFactory
    {

        private readonly Dictionary<Type, object> _commonServices = new Dictionary<Type, object>();


        public Dictionary<Type, object> CommonServices
        {
            get { return _commonServices; }
        }

        public IPluginServiceProvider Create(IDictionary<Type, object> additionalServices)
        {
            var sp = new PluginServiceProvider(CommonServices);
            foreach (var pair in additionalServices)
            {
                sp.AddService(pair.Key, pair.Value);
            }
            return sp;
        }
    }
}
