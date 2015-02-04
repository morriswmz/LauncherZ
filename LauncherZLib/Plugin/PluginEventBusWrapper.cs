using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using LauncherZLib.API;
using LauncherZLib.Event;

namespace LauncherZLib.Plugin
{
    public class PluginEventBusWrapper : IEventBus
    {

        private readonly IEventBus _eventBus;
        private readonly Dispatcher _dispatcher;

        public PluginEventBusWrapper(IEventBus eventBus, Dispatcher dispatcher)
        {
            if (eventBus == null)
                throw new ArgumentNullException("eventBus");
            if (dispatcher == null)
                throw new ArgumentNullException("dispatcher");

            _eventBus = eventBus;
            _dispatcher = dispatcher;
        }

        public void Register(object obj)
        {
            _eventBus.Register(obj);
        }

        public void Unregister(object obj)
        {
            _eventBus.Unregister(obj);
        }

        public void UnregisterAll()
        {
            _eventBus.UnregisterAll();
        }

        public void Post(EventBase e)
        {
            if (_dispatcher.CheckAccess())
            {
                _eventBus.Post(e);
            }
            else
            {
                _dispatcher.InvokeAsync(() => _eventBus.Post(e), DispatcherPriority.Send);
            }
        }

    }
}
