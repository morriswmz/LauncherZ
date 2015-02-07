using LauncherZLib.Event.Plugin;

namespace LauncherZLib.Event.PluginInternal
{
    public class QueryResultUpdateEventI : PluginEventInternal<QueryResultUpdateEvent>
    {
        public QueryResultUpdateEventI(QueryResultUpdateEvent baseEvent, string sourceId)
            : base(baseEvent, sourceId)
        {

        }
    }
}
