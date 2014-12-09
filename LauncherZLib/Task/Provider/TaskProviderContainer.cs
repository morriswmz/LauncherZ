using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Documents;
using LauncherZLib.API;

namespace LauncherZLib.Task.Provider
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

        private readonly TaskProviderEventBus _eventBus;

        public string Id { get { return _id; } }

        public string Name { get { return _name; } }

        public ReadOnlyCollection<string> Authors { get { return _authors.AsReadOnly(); } }

        public Version Version { get { return _version; } }

        public string Description { get { return _description; } }

        public double Priority { get { return _priority; } }

        public IEventBus EventBus { get { return _eventBus; } }

        public TaskProviderContainer(ITaskProvider provider, string id, string name, List<string> authors, Version version, string description, double priority)
        {
            _provider = provider;
            _id = id;
            _name = name;
            _authors = authors;
            _version = version;
            _description = description;
            _priority = priority;
            _eventBus = new TaskProviderEventBus();
        }

        public TaskProviderContainer(ITaskProvider provider, TaskProviderInfo info)
            : this(provider, info.Id, info.Name, info.Authors, new Version(info.Version), info.Description, info.Priority)
        { }
    }
}
