using System;
using System.Windows.Threading;
using LauncherZLib.Event;
using LauncherZLib.Utils;

namespace LauncherZLib.Plugin
{
    public class PluginEventBusWrapper : IEventBus
    {

        private readonly IEventBus _eventBus;
        private readonly IDispatcherService _dispatcher;

        public PluginEventBusWrapper(IEventBus eventBus, IDispatcherService dispatcherService)
        {
            if (eventBus == null)
                throw new ArgumentNullException("eventBus");
            if (dispatcherService == null)
                throw new ArgumentNullException("dispatcherService");

            _eventBus = eventBus;
            _dispatcher = dispatcherService;
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
