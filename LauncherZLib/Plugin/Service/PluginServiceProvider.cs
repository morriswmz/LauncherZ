using System;
using System.Collections.Generic;

namespace LauncherZLib.Plugin.Service
{
    

    /// <summary>
    /// Provides services to plugins.
    /// </summary>
    public sealed class PluginServiceProvider : IPluginServiceProvider
    {
        private const string ErrMessageNotInterface = "Service type must be an interface.";
        private const string ErrMessageNotAvailable = "Service \"{0}\" is not available.";

        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public PluginServiceProvider()
        {
            
        }

        public PluginServiceProvider(IDictionary<Type, object> services)
        {
            foreach (var pair in services)
            {
                AddService(pair.Key, pair.Value);
            }
        }

        public bool CanProvideService<T>() where T : class
        {
            return CanProvideService(typeof (T));
        }

        public bool CanProvideService(Type t)
        {
            return _services.ContainsKey(t);
        }

        public T GetService<T>() where T: class 
        {
            Type st = typeof (T);
            return (T) GetService(st);
        }

        public object GetService(Type t)
        {
            if (!t.IsInterface)
                throw new Exception(ErrMessageNotInterface);
            object serviceObj;
            if (!_services.TryGetValue(t, out serviceObj))
            {
                throw new Exception(string.Format(ErrMessageNotAvailable, t.FullName));
            }
            return serviceObj;
        }

        public void AddService(Type t, object service)
        {
            if (t == null)
                throw new ArgumentNullException("t");
            if (!t.IsInterface)
                throw new Exception(ErrMessageNotInterface);
            if (service == null)
                throw new ArgumentNullException("service");
            _services[t] = service;
        }


    }
}
