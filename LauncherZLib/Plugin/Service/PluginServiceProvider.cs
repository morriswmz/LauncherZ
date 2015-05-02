using System;
using System.Collections.Generic;

namespace LauncherZLib.Plugin.Service
{
    

    /// <summary>
    /// Provides services to plugins.
    /// </summary>
    public sealed class PluginServiceProvider : IExtendedServiceProvider
    {
        private const string ErrMessageNotInterface = "Service type must be an interface.";
        private const string ErrMessageNotAvailable = "Service \"{0}\" is not available.";

        private readonly Dictionary<Type, ServiceEntry> _services = new Dictionary<Type, ServiceEntry>();

        public PluginServiceProvider()
        {

        }

        public PluginServiceProvider(IEnumerable<KeyValuePair<Type, object>> services)
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

        public bool CanProvideService(Type serviceType)
        {
            return _services.ContainsKey(serviceType);
        }

        public T GetService<T>() where T: class 
        {
            Type st = typeof (T);
            return (T) GetService(st);
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");
            if (!serviceType.IsInterface)
                throw new Exception(ErrMessageNotInterface);
            ServiceEntry serviceObj;
            if (!_services.TryGetValue(serviceType, out serviceObj))
            {
                throw new Exception(string.Format(ErrMessageNotAvailable, serviceType.FullName));
            }
            if (serviceObj.Initialized)
            {
                return serviceObj.ServiceInstance;
            }
            serviceObj.ServiceInstance = serviceObj.Initializer();
            serviceObj.Initializer = null;
            return serviceObj.ServiceInstance;
        }

        public void AddService(Type serviceType, object service)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");
            if (!serviceType.IsInterface)
                throw new Exception(ErrMessageNotInterface);
            if (service == null)
                throw new ArgumentNullException("service");
            _services[serviceType] = new ServiceEntry
            {
                Initialized = true,
                ServiceInstance = service
            };
        }

        public void AddService<T>(Lazy<T> service) where T : class
        {
            if (service == null)
                throw new ArgumentNullException("service");
            Type t = typeof (T);
            if (!t.IsInterface)
                throw new Exception(ErrMessageNotInterface);
            _services[t] = new ServiceEntry
            {
                Initialized = false,
                Initializer = () => service.Value
            };
        }

        public void AddService<T>(Func<T> serviceInitializer) where T : class
        {
            if (serviceInitializer == null)
                throw new ArgumentNullException("serviceInitializer");
            Type t = typeof(T);
            if (!t.IsInterface)
                throw new Exception(ErrMessageNotInterface);
            _services[t] = new ServiceEntry
            {
                Initialized = false,
                Initializer = serviceInitializer
            };
        }

        sealed class ServiceEntry
        {
            public bool Initialized { get; set; }
            public object ServiceInstance { get; set; }
            public Func<object> Initializer { get; set; }
        }
    }
}
