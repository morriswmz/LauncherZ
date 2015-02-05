using System;
using LauncherZLib.API;

namespace LauncherZLib.Event.Plugin
{
    /// <summary>
    /// Based class for all events that can be raised by plugins.
    /// </summary>
    public abstract class PluginSourcedEvent : EventBase
    {
        protected readonly IPluginContext SourceContext;

        public IPluginContext SourcePluginContext { get { return SourceContext; } }

        protected PluginSourcedEvent(IPluginContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            SourceContext = context;
        }

    }
}
