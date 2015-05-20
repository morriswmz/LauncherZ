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
        private readonly List<string> _searchDirectories = new List<string>();


        public PluginDiscoverer(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            _logger = logger;
        }

        public List<string> SearchDirectories
        {
            get { return _searchDirectories; }
        }

        public IEnumerable<PluginDiscoveryInfo> DiscoverAll()
        {
            var result = new List<PluginDiscoveryInfo>();
            // todo: properly setup a sandboxed domain
            string executingAssemblyName = Assembly.GetExecutingAssembly().FullName;
            AppDomain discoveryDomain = AppDomain.CreateDomain("PluginDiscoveryDomain");
            var asmDiscoverer = (AssemblyDiscoverer)discoveryDomain.CreateInstanceAndUnwrap(executingAssemblyName, typeof(AssemblyDiscoverer).ToString());
            asmDiscoverer.ConfigureAppDomain(new string[] {Assembly.GetExecutingAssembly().Location});

            foreach (var dir in SearchDirectories)
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
                        result.AddRange(asmDiscoverer.DiscoverPluginsInAssembly(fi.FullName));
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
                Assembly asm = null;
                try
                {
                    asm = Assembly.ReflectionOnlyLoadFrom(path);
                }
                catch (Exception ex)
                {
                    return Enumerable.Empty<PluginDiscoveryInfo>();
                }
                var result = new List<PluginDiscoveryInfo>();
                Type pluginAttrType = Type.ReflectionOnlyGetType(typeof (PluginAttribute).AssemblyQualifiedName, true, false);
                Type descriptionType = Type.ReflectionOnlyGetType(typeof (DescriptionAttribute).AssemblyQualifiedName, true, false);
                Type iPluginType = Type.ReflectionOnlyGetType(typeof (IPlugin).AssemblyQualifiedName, true, false);
                foreach (Type t in asm.GetTypes())
                {
                    IList<CustomAttributeData> attrs = CustomAttributeData.GetCustomAttributes(t);
                    if (attrs.Count == 0)
                        continue;
                    // find desired attributes
                    CustomAttributeData pluginAttrData = null;
                    CustomAttributeData descriptionAttrData = null;
                    foreach (var attr in attrs)
                    {
                        if (attr.AttributeType == pluginAttrType)
                        {
                            pluginAttrData = attr;
                        }
                        else if (attr.AttributeType == descriptionType)
                        {
                            descriptionAttrData = attr;
                        }
                    }
                    if (pluginAttrData == null)
                        continue;
                    // init plugin discovery information
                    var pdi = new PluginDiscoveryInfo
                    {
                        AssemblyPath = path,
                        Type = PluginType.Assembly,
                        PluginClass = t.FullName,
                        SourceDirectory = Path.GetDirectoryName(path)
                    };
                    var errMessages = new List<string>();
                    // parse data in attributes
                    PluginAttribute pluginAttr;
                    try
                    {
                        pluginAttr = CreatePluginAttributeFromData(pluginAttrData);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    DescriptionAttribute descriptionAttr = null;
                    if (descriptionAttrData != null)
                    {
                        try
                        {
                            descriptionAttr = CreateDescriptionAttributeFromData(descriptionAttrData);
                        }
                        catch (Exception ex)
                        {
                            descriptionAttr = null;
                        }
                    }
                    // verify and read id
                    pdi.Id = pluginAttr.Id;
                    if (!pdi.Id.IsProperId())
                    {
                        errMessages.Add("Invalid plugin id.");
                    }
                    else if (pdi.Id.Equals("LauncherZ", StringComparison.OrdinalIgnoreCase))
                    {
                        errMessages.Add("Plugin id cannot be \"LauncherZ\"");
                    }
                    // verify and read version
                    Version pluginVersion;
                    if (!Version.TryParse(pluginAttr.Version, out pluginVersion))
                    {
                        errMessages.Add("Incorrect version format.");
                    }
                    else
                    {
                        pdi.PluginVersion = pluginVersion;
                    }
                    // read remaining fields
                    pdi.FriendlyName = pluginAttr.FriendlyName;
                    pdi.Authors = pluginAttr.Authors.Split(',').Select(a => a.Trim()).ToArray();
                    if (descriptionAttr != null)
                    {
                        pdi.Description = descriptionAttr.Description;
                    }
                    // finally, check class
                    if (!t.IsClass)
                    {
                        errMessages.Add("Plugin class is not a class.");
                    }
                    else if (!t.IsPublic)
                    {
                        errMessages.Add("Plugin class must be public.");
                    }
                    else if (t.IsAbstract)
                    {
                        errMessages.Add("Plugin class cannot be abstract.");
                    }
                    else if (t.IsGenericType)
                    {
                        errMessages.Add("Plugin class cannot be generic.");
                    }
                    else if (!iPluginType.IsAssignableFrom(t))
                    {
                        errMessages.Add("Plugin class does not implement IPlugin interface.");
                    }
                    // set validity
                    pdi.IsValid = errMessages.Count == 0;
                    pdi.ErrorMessage = string.Join(" ", errMessages);
                    result.Add(pdi);
                }

                return result;
            }

            private PluginAttribute CreatePluginAttributeFromData(CustomAttributeData attr)
            {
                var id = (attr.ConstructorArguments[0].Value as string) ?? "";
                var pluginAttr = new PluginAttribute(id);
                if (attr.NamedArguments == null || attr.NamedArguments.Count == 0)
                    return pluginAttr;
                foreach (var namedArgument in attr.NamedArguments)
                {
                    switch (namedArgument.MemberName)
                    {
                        case "FriendlyName":
                            pluginAttr.FriendlyName = namedArgument.TypedValue.Value as string;
                            break;
                        case "Authors":
                            pluginAttr.Authors = namedArgument.TypedValue.Value as string;
                            break;
                        case "Version":
                            pluginAttr.Version = namedArgument.TypedValue.Value as string;
                            break;
                    }
                }
                return pluginAttr;
            }

            private DescriptionAttribute CreateDescriptionAttributeFromData(CustomAttributeData attr)
            {
                var description = attr.ConstructorArguments[0].Value as string;
                return description == null ? DescriptionAttribute.Default : new DescriptionAttribute(description);
            }

        }

    }
}
