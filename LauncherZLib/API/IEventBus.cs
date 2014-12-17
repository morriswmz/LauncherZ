using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.Event;

namespace LauncherZLib.API
{

    public interface IEventBus
    {
        void Register(object obj);

        void Unregister(object obj);

        void UnregisterAll();

        void Post(EventBase e);
    }
}
