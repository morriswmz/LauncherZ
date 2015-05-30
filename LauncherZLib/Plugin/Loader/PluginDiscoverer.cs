using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using LauncherZLib.Utils;

namespace LauncherZLib.Plugin.Loader
{
    /// <summary>
    /// Discovers plugins.
    /// </summary>
    public sealed class PluginDiscoverer
    {

        private readonly ILogger _logger;
        
        public PluginDiscoverer(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            _logger = logger;
        }

        public IEnumerable<PluginDiscoveryInfo> DiscoverAllIn(IEnumerable<string> searchDirectories)
        {
            var result = new List<PluginDiscoveryInfo>();
            // todo: properly setup a sandboxed domain
            string executingAssemblyName = Assembly.GetExecutingAssembly().FullName;
            AppDomain discoveryDomain = AppDomain.CreateDomain("PluginDiscoveryDomain");
            var asmDiscoverer = (AssemblyDiscoverer)discoveryDomain.CreateInstanceAndUnwrap(executingAssemblyName, typeof(AssemblyDiscoverer).ToString());
            asmDiscoverer.ConfigureAppDomain(new string[] {Assembly.GetExecutingAssembly().Location});

            foreach (var dir in searchDirectories)
            {
                string directory = Path.GetFullPath(dir);
                if (!Directory.Exists(directory))
                {
                    _logger.Warning("Specified path \"{0}\" does not exist", directory);
                    continue;
                }
                var walker = new SafeDirectoryWalker
                {
                    Recursive = true,
                    MaxDepth = 2,
                    SearchPattern = new Regex(@".dll$", RegexOptions.IgnoreCase)
                };
                _logger.Info("Searching \"{0}\" recursively for plugins.", directory);
                walker.Walk(directory, fi =>
                {
                    try
                    {
                        result.AddRange(asmDiscoverer.DiscoverPluginsInAssembly(fi.FullName).Where(x => x.IsValid));
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(
                            "An exception occurred while searching for plugins in \"{0}\". Details:{1}{2}",
                            fi.FullName, Environment.NewLine, ex);
                    }
                    return true;
                });
            }
            
            AppDomain.Unload(discoveryDomain);
            return result;
        }

        

        private class AssemblyDiscoverer : MarshalByRefObject
        {

            public void ConfigureAppDomain(string[] preloadedAssemlies)
            {
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (sender, args) => 
                    Assembly.ReflectionOnlyLoad(args.Name);
                foreach (var asm in preloadedAssemlies)
                {
                    Assembly.ReflectionOnlyLoad(AssemblyName.GetAssemblyName(asm).FullName);
                }
            }

            public IEnumerable<PluginDiscoveryInfo> DiscoverPluginsInAssembly(string path)
            {
                try
                {
                    Type reflOnlyIPluginType =
                        Type.ReflectionOnlyGetType((typeof (IPlugin)).AssemblyQualifiedName, true, false);
                    Assembly asm = Assembly.ReflectionOnlyLoadFrom(path);
                    return asm.GetTypes()
                        .Where(reflOnlyIPluginType.IsAssignableFrom)
                        .Select(x => PluginDiscoveryInfo.FromType(x, true))
                        .ToArray();
                }
                catch (Exception ex)
                {
                    return Enumerable.Empty<PluginDiscoveryInfo>();
                }
            }
        }

    }
}
