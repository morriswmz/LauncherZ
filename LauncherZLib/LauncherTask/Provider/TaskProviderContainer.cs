using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using LauncherZLib.API;
using LauncherZLib.Event;

namespace LauncherZLib.LauncherTask.Provider
{
    
    public sealed class TaskProviderContainer
    {

        private readonly ITaskProvider _provider;
        private readonly string _id;
        private readonly List<string> _authors;
        private readonly Version _version;
        private readonly string _description;
        private readonly double _priority;
        private readonly string _sourceDirectory;
        private readonly string _dataDirectory;
        private readonly bool _isAsync;

        private readonly EventBus _eventBus;    

        public string Id { get { return _id; } }

        public ReadOnlyCollection<string> Authors { get { return _authors.AsReadOnly(); } }

        public Version Version { get { return _version; } }

        public string Description { get { return _description; } }

        public double Priority { get { return _priority; } }

        public string SourceDirectory { get { return _sourceDirectory; } }

        public string DataDirectory { get { return _dataDirectory; } }

        public ITaskProvider Provider { get { return _provider; } }

        public IEventBus EventBus { get { return _eventBus; } }

        public bool IsAsync { get { return _isAsync; } }

        public TaskProviderContainer(ITaskProvider provider, string id, List<string> authors, Version version, string description, double priority, string sourceDir, string dataDir)
        {
            _provider = provider;
            _id = id;
            _authors = authors;
            _version = version;
            _description = description;
            _priority = priority;
            _sourceDirectory = sourceDir;
            _dataDirectory = dataDir;
            _eventBus = new EventBus();
            _isAsync = provider is ITaskProviderAsync;
        }

        public TaskProviderContainer(ITaskProvider provider, TaskProviderInfo info)
            : this(provider, info.Id, info.Authors, new Version(info.Version), info.Description, info.Priority, info.SourceDirectory, info.DataDirectory)
        { }

        public string GetLocalizedName(CultureInfo cultureInfo)
        {
            return _provider.GetLocalizedName(cultureInfo);
        }

        public string GetLocalizedName()
        {
            return _provider.GetLocalizedName(CultureInfo.CurrentCulture);
        }

        public override string ToString()
        {
            string authors = string.Join(", ", _authors);
            return string.Format("[Name={0}, Version={1}, Authors=[{2}]]", GetLocalizedName(), _version, authors);
        }
    }
}
