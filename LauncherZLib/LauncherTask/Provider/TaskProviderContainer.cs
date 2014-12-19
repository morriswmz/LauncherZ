using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using LauncherZLib.API;
using LauncherZLib.Event;
using LauncherZLib.I18N;

namespace LauncherZLib.LauncherTask.Provider
{
    
    public sealed class TaskProviderContainer
    {

        private readonly ITaskProvider _provider;
        private readonly string _id;
        private readonly string _name;
        private readonly List<string> _authors;
        private readonly Version _version;
        private readonly string _description;
        private readonly double _priority;
        private readonly string _sourceDirectory;
        private readonly string _dataDirectory;
        private readonly bool _isAsync;

        private readonly EventBus _eventBus;
        private readonly LocalizationDictionary _locDict;

        public string Id { get { return _id; } }

        public string Name
        {
            get
            {
                return _locDict.CanTranslate("Name") ? _locDict["Name"] : _name;
            }
        }

        public ReadOnlyCollection<string> Authors { get { return _authors.AsReadOnly(); } }

        public Version Version { get { return _version; } }

        public string Description
        {
            get
            {
                return _locDict.CanTranslate("Description") ? _locDict["Description"] : _description;
            }
        }

        public double Priority { get { return _priority; } }

        public string SourceDirectory { get { return _sourceDirectory; } }

        public string DataDirectory { get { return _dataDirectory; } }

        public ITaskProvider Provider { get { return _provider; } }

        public IEventBus EventBus { get { return _eventBus; } }

        public LocalizationDictionary I18N { get { return _locDict; } }

        public bool IsAsync { get { return _isAsync; } }

        public TaskProviderContainer(ITaskProvider provider, string id, string name, List<string> authors, Version version, string description, double priority, string sourceDir, string dataDir)
        {
            _provider = provider;
            _id = id;
            _name = name;
            _authors = authors;
            _version = version;
            _description = description;
            _priority = priority;
            _sourceDirectory = sourceDir;
            _dataDirectory = dataDir;
            _eventBus = new EventBus();
            _locDict=new LocalizationDictionary();
            _isAsync = provider is ITaskProviderAsync;
        }

        public TaskProviderContainer(ITaskProvider provider, TaskProviderInfo info)
            : this(provider, info.Id, info.Name, info.Authors, new Version(info.Version), info.Description, info.Priority, info.SourceDirectory, info.DataDirectory)
        { }

        public override string ToString()
        {
            string authors = string.Join(", ", _authors);
            return string.Format("[Name={0}, Version={1}, Authors=[{2}]]", Name, _version, authors);
        }
    }
}
