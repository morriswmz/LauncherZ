using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LauncherZLib.Plugin;
using LauncherZLib.Utils;

namespace LauncherZLib.Launcher
{
    /// <summary>
    /// Manages query results.
    /// </summary>
    public sealed class ResultManager
    {
        private readonly PluginManager _pluginManager;

        private readonly ObservableCollection<LauncherData> _results = new ObservableCollection<LauncherData>();
        private readonly Dictionary<LauncherData, string> _pluginIdMap = new Dictionary<LauncherData, string>();

        public ResultManager(PluginManager pluginManager, QueryDistributor distributor)
        {
            if (pluginManager == null)
                throw new ArgumentNullException("pluginManager");
            if (distributor == null)
                throw new ArgumentNullException("distributor");

            _pluginManager = pluginManager;
            Results = new ReadOnlyObservableCollection<LauncherData>(_results);

            distributor.QueryReset += Distributor_QueryReset;
            distributor.ResultUpdate += Distributor_ResultUpdate; 
        }

        public int ResultCount
        {
            get { return _results.Count; }
        }

        public ReadOnlyObservableCollection<LauncherData> Results { get; private set; }

        public string GetPluginIdOf(LauncherData launcherData)
        {
            return _pluginIdMap.ContainsKey(launcherData) ? _pluginIdMap[launcherData] : null;
        }

        private void AddResult(TaggedObject<LauncherData> taggedData)
        {
            _pluginIdMap[taggedData.Object] = taggedData.Tag;
            if (ResultCount == 0)
            {
                _results.Add(taggedData.Object);
                return;
            }
            // perform binary search and insert
            int l = 0; // highest priority
            int h = ResultCount - 1; // lowest priority
            while (l < h)
            {
                int m = (l + h) / 2;
                int result = CompareLauncherData(taggedData.Object, _results[m]);
                if (result > 0)
                {
                    h = m - 1;
                }
                else if (result < 0)
                {
                    l = m + 1;
                }
                else
                {
                    l = h = m;
                }
            }
            // for equal priority, we apply the first-come first-served rule
            // l will be the index for insertion
            int n = ResultCount;
            while (l < n && CompareLauncherData(taggedData.Object, _results[l]) <= 0)
            {
                l++;
            }
            _results.Insert(l, taggedData.Object);
        }

        private int CompareLauncherData(LauncherData x, LauncherData y)
        {
            if (x.Relevance > y.Relevance)
                return 1;

            if (x.Relevance < y.Relevance)
                return -1;

            double xp = _pluginManager.GetPluginPriority(GetPluginIdOf(x));
            double yp = _pluginManager.GetPluginPriority(GetPluginIdOf(y));
            return xp.CompareTo(yp);
        }

        private void Distributor_ResultUpdate(object sender, ResultUpdatedEventArgs e)
        {
            foreach (var taggedData in e.Updates)
            {
                AddResult(taggedData);
            }
        }

        private void Distributor_QueryReset(object sender, EventArgs eventArgs)
        {
            _results.Clear();
            // must clear id map after, otherwise event handlers will not be able to access plugin id
            _pluginIdMap.Clear();
        }

    }
}
