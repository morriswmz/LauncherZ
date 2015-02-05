using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using LauncherZLib.API;
using LauncherZLib.Event;
using LauncherZLib.I18N;
using LauncherZLib.Launcher;

namespace LauncherZLib.Plugin
{

    /// <summary>
    /// Contains a plugin, stores its information and provides a IPluginContext.
    /// </summary>
    public sealed class PluginContainer : IPluginContext, IComparable<PluginContainer>
    {

        private readonly IPlugin _plugin;
        private readonly string _id;
        private readonly string _name;
        private readonly List<string> _authors;
        private readonly Version _version;
        private readonly string _description;
        private double _priority = 0.0;
        private readonly string _sourceDirectory;
        private readonly string _suggestedDataDirectory;
        private readonly bool _isAsync;

        private readonly IDispatcherService _dispatcherService;
        private readonly EventBus _eventBus;
        private readonly PluginEventBusWrapper _eventBusWrapper;
        private readonly PluginEventRelay _eventRelay;
        private readonly LocalizationDictionary _locDict;
        private readonly ILogger _logger;

        
        private bool _activiting;

        /// <summary>
        /// Gets the id of the plugin, as defined in the manifest file.
        /// </summary>
        public string Id { get { return _id; } }

        /// <summary>
        /// Gets the localized name of the plugin.
        /// If LocalizationDictionary cannot translate the "Name", will return the
        /// plugin name defined in the manifest file.
        /// </summary>
        public string Name
        {
            get
            {
                return _locDict.CanTranslate("Name") ? _locDict["Name"] : _name;
            }
        }

        /// <summary>
        /// Gets a collection of authors of this plugin, as defined in the manifest file.
        /// </summary>
        public ReadOnlyCollection<string> Authors { get { return _authors.AsReadOnly(); } }

        /// <summary>
        /// 
        /// </summary>
        public Version Version { get { return _version; } }

        /// <summary>
        /// 
        /// </summary>
        public string Description
        {
            get
            {
                return _locDict.CanTranslate("Description") ? _locDict["Description"] : _description;
            }
        }
        
        /// <summary>
        /// Gets or sets the priority of this plugin.
        /// Value is constrained between 0 and 1 inclusive.
        /// </summary>
        public double Priority { 
            get { return _priority; }
            set {
                _priority = double.IsNaN(value) ? 0.0 : Math.Max(0.0, Math.Min(value, 1.0));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string SourceDirectory { get { return _sourceDirectory; } }

        /// <summary>
        /// 
        /// </summary>
        public string SuggestedDataDirectory { get { return _suggestedDataDirectory; } }

        /// <summary>
        /// 
        /// </summary>
        public IPlugin Plugin { get { return _plugin; } }

        /// <summary>
        /// 
        /// </summary>
        public IEventBus EventBus { get { return _eventBusWrapper; } }

        /// <summary>
        /// 
        /// </summary>
        internal IEventBus UnwrappedEventBus { get { return _eventBus; } }

        /// <summary>
        /// 
        /// </summary>
        public LocalizationDictionary Localization { get { return _locDict; } }

        /// <summary>
        /// 
        /// </summary>
        public bool IsAsync { get { return _isAsync; } }

        public PluginStatus Status { get; private set; }

        /// <summary>
        /// Gets the logger created for this plugin.
        /// </summary>
        public ILogger Logger { get { return _logger; } }

        /// <summary>
        /// 
        /// </summary>
        internal Action<string, string> CrashHandler { get; set; } 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plugin"></param>
        /// <param name="info"></param>
        /// <param name="logger"></param>
        /// <param name="dispatcher"></param>
        public PluginContainer(IPlugin plugin, PluginInfo info, PluginContextParameters contextParams)
        {
            if (plugin == null)
                throw new ArgumentNullException("plugin");
            if (info == null)
                throw new ArgumentNullException("info");
            if (contextParams == null)
                throw new ArgumentNullException("contextParams");

            _plugin = plugin;
            // copy plugin information
            _id = info.Id;
            _name = info.Name;
            _authors = info.Authors;
            _version = new Version(info.Version);
            _description = info.Description;
            _sourceDirectory = info.SourceDirectory;
            _suggestedDataDirectory = info.DataDirectory;

            // set up plugin environment
            if (contextParams.DispatcherService == null)
                throw new NullReferenceException("DispatcherService cannot be null in PluginContextParameters.");
            if (contextParams.Logger == null)
                throw new NullReferenceException("Logger cannot be null in PluginContextParameters.");
            if (contextParams.ParentEventBus == null)
                throw new NullReferenceException("ParentEventBus cannot be null in PluginContextParameters.");

            _dispatcherService = contextParams.DispatcherService;
            _eventBus = new EventBus();
            _eventBusWrapper = new PluginEventBusWrapper(_eventBus, _dispatcherService);
            _eventRelay = new PluginEventRelay(contextParams.ParentEventBus, _eventBus);
            _logger = contextParams.Logger;
            _locDict = new LocalizationDictionary();
            
            Status = PluginStatus.Deactivated;
        }

        public IEnumerable<LauncherData> Query(LauncherQuery query)
        {
            if (Status != PluginStatus.Activated)
                throw new InvalidOperationException();

            IEnumerable<LauncherData> results = _plugin.Query(query);
            return results ?? Enumerable.Empty<LauncherData>();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Activate()
        {
            if (Status != PluginStatus.Deactivated)
                return;

            _plugin.Activate(this);
            _eventRelay.Link();
            Status = PluginStatus.Activated;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Deactivate()
        {
            if (Status != PluginStatus.Activated)
                return;

            Status = PluginStatus.Deactivated;
            // always mark as deactivated, despite exceptions
            _eventRelay.Unlink();
            _plugin.Deactivate(this);
        }

        public void SendCrashNotification(string friendlyMsg)
        {
            CrashHandler(_id, friendlyMsg);
        }

        public void DoCrashCleanup()
        {
            Status = PluginStatus.Crashed;
        }

        public int CompareTo(PluginContainer other)
        {
            return _priority.CompareTo(other._priority);
        }

        public override string ToString()
        {
            string authors = string.Join(", ", _authors);
            return string.Format("{{Name={0}, Version={1}, Authors=[{2}]}}", Name, _version, authors);
        }
    }
}
