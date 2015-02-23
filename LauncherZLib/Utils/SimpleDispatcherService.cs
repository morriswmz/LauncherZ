using System;
using System.Windows.Threading;

namespace LauncherZLib.Utils
{
    public class SimpleDispatcherService : IDispatcherService
    {
        protected readonly Dispatcher InternalDispatcher;

        public SimpleDispatcherService(Dispatcher dispatcher)
        {
            if (dispatcher == null)
                throw new ArgumentNullException("dispatcher");

            InternalDispatcher = dispatcher;
        }

        public DispatcherOperation InvokeAsync(Action callback)
        {
            return InternalDispatcher.InvokeAsync(callback);
        }

        public DispatcherOperation InvokeAsync(Action callback, DispatcherPriority priority)
        {
            return InternalDispatcher.InvokeAsync(callback, priority);
        }

        public bool CheckAccess()
        {
            return InternalDispatcher.CheckAccess();
        }
    }
}
