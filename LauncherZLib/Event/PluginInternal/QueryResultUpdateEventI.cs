using LauncherZLib.Event.Plugin;

namespace LauncherZLib.Event.PluginInternal
{
    public class QueryResultUpdateEventI : PluginEventInternal<QueryResultUpdateEvent>
    {
        public QueryResultUpdateEventI(string sourceId, QueryResultUpdateEvent baseEvent)
            : base(sourceId, baseEvent)
        {

        }
    }
}
