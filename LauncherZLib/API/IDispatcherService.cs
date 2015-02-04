using System;
using System.Windows.Threading;

namespace LauncherZLib.API
{
    /// <summary>
    /// Exposes asynchronous invocation and access checking of a Dispatcher.
    /// </summary>
    /// <seealso cref="T:System.Windows.Threading.Dispatcher"/>
    public interface IDispatcherService
    {

        DispatcherOperation InvokeAsync(Action callback);

        DispatcherOperation InvokeAsync(Action callback, DispatcherPriority priority);

        bool CheckAccess();

    }
}
