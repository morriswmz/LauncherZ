using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Windows.Documents;
using LauncherZLib.API;
using LauncherZLib.Utils;
using Newtonsoft.Json;

namespace LauncherZLib.Task.Provider
{
    public class TaskProviderLoader
    {
        private readonly Dictionary<string, TaskProviderContainer> _loadedProviders = new Dictionary<string, TaskProviderContainer>();
        private readonly List<string> _disabledProviders = new List<string>();

        public Dictionary<string, TaskProviderContainer> Providers { get { return _loadedProviders; } } 

        public void LoadAllFrom(string searchPath)
        {
            if (!Directory.Exists(searchPath))
                return;

            string[] directories = Directory.GetDirectories(searchPath);
            var result = new List<TaskProviderContainer>();
            foreach (string dir in directories)
            {
                //try
                {
                    DiscoverAndLoad(dir);
                }
                //catch (Exception)
                {
                    // todo: log it
                }
            }

        }

        public void DiscoverAndLoad(string path)
        {
            string jsonPath = path + @"\manifest.json";
            if (!File.Exists(jsonPath))
            {
                // todo: log this
                return;
            }

            // read manifest.json
            string json = File.ReadAllText(jsonPath);
            TaskProviderManifest manifest = JsonConvert.DeserializeObject<TaskProviderManifest>(json);

            // check and load
            foreach (var info in manifest.Providers)
            {
                if (string.IsNullOrEmpty(info.Id) )
                {
                    // invalid id
                    // todo : log this
                }
                else if (_loadedProviders.ContainsKey(info.Id))
                {
                    // todo : log this
                }
                else if (!_disabledProviders.Contains(info.Id))
                {
                    if (!info.Id.IsProperId())
                    {
                        // todo: log this
                    }

                    ITaskProvider provider = null;
                    if (info.ProviderType == TaskProviderType.Assembly)
                    {
                        provider = LoadAssemblyProvider(info, path);
                    } else if (info.ProviderType == TaskProviderType.Xml)
                    {
                        provider = LoadXmlProvider(info, path);
                    } else if (info.ProviderType == TaskProviderType.Command)
                    {
                        provider = LoadCommandLineProvider(info, path);
                    }
                    else
                    {
                        // todo: log this
                    }


                    if (provider != null)
                    {
                        var container = new TaskProviderContainer(provider, info);
                        provider.Initialize(container.EventBus);
                        _loadedProviders.Add(info.Id, new TaskProviderContainer(provider, info));
                        
                    }
                }
            }
        }

        private static ITaskProvider LoadAssemblyProvider(TaskProviderInfo info, string directory)
        {
            string assemblyPath = directory + '\\' + (info.Assembly.StartsWith(".\\")
                ? info.Assembly.Substring(2)
                : info.Assembly);
            if (!File.Exists(assemblyPath))
            {
                // todo: log this
                return null;
            }
            return Activator.CreateInstanceFrom(assemblyPath, info.ProviderClass).Unwrap() as ITaskProvider;
        }

        private static ITaskProvider LoadXmlProvider(TaskProviderInfo info, string directory)
        {
            return null;
        }

        private static ITaskProvider LoadCommandLineProvider(TaskProviderInfo info, string directory)
        {
            return null;
        }

    }

}
