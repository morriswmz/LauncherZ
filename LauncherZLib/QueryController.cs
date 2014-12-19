using System.Collections.Generic;
using System.Collections.ObjectModel;
using LauncherZLib.API;
using LauncherZLib.LauncherTask;
using LauncherZLib.LauncherTask.Provider;

namespace LauncherZLib
{
    public class QueryController
    {

        private readonly TaskProviderManager _providerManager;
        private Dictionary<string, TaskProviderContainer> _taskProviderMap;
        private Dictionary<string, double> _priorityMap;
        private int _maxResults;

        private ObservableCollection<TaskData> _results;
        private ReadOnlyObservableCollection<TaskData> _resultsReadOnly;

        public QueryController(TaskProviderManager manager, int maxResults)
        {
            _providerManager = manager;
            _maxResults = maxResults;
            _results = new ObservableCollection<TaskData>();
            _resultsReadOnly = new ReadOnlyObservableCollection<TaskData>(_results);
            
        }

        public ReadOnlyObservableCollection<TaskData> Results { get { return _resultsReadOnly; } }

        public void DistributeQuery(TaskQuery query)
        {
            _results.Clear();
            foreach (var container in _providerManager.ActiveProviders)
            {
                if (_results.Count > _maxResults)
                    break;;
                
            }
        }

        public void ClearCurrentQuery()
        {
            _results.Clear();
        }

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

        /// <summary>
        /// Handles asynchrounous results.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AysncResultHandler(object sender, UpdateEventArgs e)
        {
            
        }

    }
}
