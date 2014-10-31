using System.Collections.Generic;
using LauncherZLib.Task.Provider;

namespace LauncherZLib.Task
{
    internal class QueryBus
    {

        private Dictionary<string, TaskProviderContainer> _taskProviderMap;
        private Dictionary<string, double> _priorityMap;

        public bool RegisterTaskProvider(TaskProviderContainer provider)
        {
            if (_taskProviderMap.ContainsKey(provider.Id))
                return false;
            _taskProviderMap.Add(provider.Id, provider);
            return true;
        }

        public double GetPriority(string providerId)
        {
            double p = -1.0;
            _priorityMap.TryGetValue(providerId, out p);
            return p;
        }

    }
}
