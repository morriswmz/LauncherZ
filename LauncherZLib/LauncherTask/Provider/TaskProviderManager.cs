using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using LauncherZLib.API;
using LauncherZLib.Event;
using LauncherZLib.Icon;
using LauncherZLib.Utils;
using Newtonsoft.Json;

namespace LauncherZLib.LauncherTask.Provider
{
    public sealed class TaskProviderManager : IAutoCompletionProvider, IIconLocationResolver
    {
        private readonly Dictionary<string, TaskProviderContainer> _loadedProviders = new Dictionary<string, TaskProviderContainer>();
        private readonly List<TaskProviderContainer> _sortedActiveProviders = new List<TaskProviderContainer>(); 
        private readonly List<string> _disabledProviders = new List<string>();

        public ReadOnlyCollection<TaskProviderContainer> ActiveProviders
        {
            get { return _sortedActiveProviders.AsReadOnly(); }
        }

        public void LoadAllFrom(string searchPath, string dataPath, SimpleLogger logger)
        {
            if (!Directory.Exists(searchPath))
                return;
            // trim trailing slash
            searchPath = searchPath.TrimEnd(Path.DirectorySeparatorChar);
            dataPath = dataPath.TrimEnd(Path.DirectorySeparatorChar);

            string[] directories = Directory.GetDirectories(searchPath);
            var result = new List<TaskProviderContainer>();
            foreach (string dir in directories)
            {
                var infos = new List<TaskProviderInfo>();
                try
                {
                    infos.AddRange(LoadManifestFrom(dir));
                }
                catch (Exception ex)
                {
                    if (logger != null)
                        logger.Error(string.Format("An exception occurr while reading manifest file from: {0}. Details:{1}{2}", dir, Environment.NewLine, ex));
                }
                if (infos.Count == 0 && logger != null)
                {
                    logger.Warning(string.Format("Empty manifest file found: {0}", dir));
                    continue;
                }
                foreach (var info in infos)
                {
                    info.SourceDirectory = dir;
                    info.DataDirectory = string.Format("{0}{1}{2}",
                        dataPath, Path.DirectorySeparatorChar, (new DirectoryInfo(dir)).Name);
                    try
                    {
                        TaskProviderContainer container = LoadProvider(info);
                        if (container != null)
                        {
                            container.Provider.Initialize(container.EventBus);
                            _loadedProviders.Add(container.Id, container);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (logger != null)
                            logger.Error(string.Format("An exception occurr while loading plugin from: {0}. Details:{1}{2}", dir, Environment.NewLine, ex));
                    }
                }
                
            }

            // activate
            foreach (var container in _loadedProviders.Values)
            {
                if (!_disabledProviders.Contains(container.Id))
                    _sortedActiveProviders.Add(container);
            }
            _sortedActiveProviders.Sort((a, b) => b.Priority.CompareTo(a.Priority));

        }

        public void DistributeEvent(EventBase e)
        {
            foreach (var container in _sortedActiveProviders)
            {
                container.EventBus.Post(e);
            }
        }

        private IEnumerable<TaskProviderInfo> LoadManifestFrom(string dir)
        {
            string jsonPath = string.Format("{0}{1}{2}", dir, Path.DirectorySeparatorChar, "manifest.json");
            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException("\"manifest.json\" not found.");
            }

            // read manifest.json
            string json = File.ReadAllText(jsonPath);
            var manifest = JsonConvert.DeserializeObject<TaskProviderManifest>(json);
            return (manifest == null || manifest.Providers == null) ? Enumerable.Empty<TaskProviderInfo>() : manifest.Providers;
        }

        private TaskProviderContainer LoadProvider(TaskProviderInfo info)
        {
            if (string.IsNullOrEmpty(info.Id))
            {
                throw new FormatException("Id is empty or missing.");
            }
            else if (_loadedProviders.ContainsKey(info.Id))
            {
                var message = string.Format(
                    "Id \"{0}\" is already used by the following plugin: {1}.",
                    info.Id, _loadedProviders[info.Id].ToString());
                throw new Exception(message);
            }
            else if (!_disabledProviders.Contains(info.Id))
            {
                if (!info.Id.IsProperId())
                {
                    throw new FormatException(string.Format("Invalid id \"{0}\".", info.Id));
                }

                ITaskProvider provider = null;
                if (info.ProviderType == TaskProviderType.Assembly)
                {
                    provider = LoadAssemblyProvider(info);
                }
                else if (info.ProviderType == TaskProviderType.Xml)
                {
                    provider = LoadXmlProvider(info);
                }
                else if (info.ProviderType == TaskProviderType.Command)
                {
                    provider = LoadCommandLineProvider(info);
                }
                else
                {
                    // todo: log this
                }

                return provider == null ? null : new TaskProviderContainer(provider, info);
            }
            else
            {
                return null;
            }
        }


        private static ITaskProvider LoadAssemblyProvider(TaskProviderInfo info)
        {
            // normalize
            if (info.Assembly.StartsWith(".\\") || info.Assembly.StartsWith(".//"))
                info.Assembly = info.Assembly.Substring(2);
            string assemblyPath = string.Format("{0}{1}{2}",
                info.SourceDirectory, Path.DirectorySeparatorChar, info.Assembly);
            if (!File.Exists(assemblyPath))
            {
                // todo: log this
                return null;
            }
            return Activator.CreateInstanceFrom(assemblyPath, info.ProviderClass).Unwrap() as ITaskProvider;
        }

        private static ITaskProvider LoadXmlProvider(TaskProviderInfo info)
        {
            return null;
        }

        private static ITaskProvider LoadCommandLineProvider(TaskProviderInfo info)
        {
            return null;
        }


        public IEnumerable<string> GetAutoCompletions(string context, int limit)
        {
            throw new NotImplementedException();
        }

        public bool TryResolve(IconLocation location, out string path)
        {
            throw new NotImplementedException();
        }
    }

}
