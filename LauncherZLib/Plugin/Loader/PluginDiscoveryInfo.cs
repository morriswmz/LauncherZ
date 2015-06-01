using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using LauncherZLib.Utils;

namespace LauncherZLib.Plugin.Loader
{
    /// <summary>
    /// Contains information of a discovered plugin candidate.
    /// </summary>
    [Serializable]
    public class PluginDiscoveryInfo
    {
        private static readonly Version DefaultVersion = new Version(0, 0, 0, 0);

        private static readonly Type ReflOnlyPluginAttrType =
            Type.ReflectionOnlyGetType((typeof (PluginAttribute)).AssemblyQualifiedName, true, false);

        private static readonly Type ReflOnlyDescriptionAttrType =
            Type.ReflectionOnlyGetType((typeof (DescriptionAttribute)).AssemblyQualifiedName, true, false);

        public PluginDiscoveryInfo()
        {
            Id = "";
            FriendlyName = "";
            PluginVersion = DefaultVersion;
            Authors = new string[0];
            Description = "";

            PluginType = "Unknown";
            AssemblyPath = "";
            PluginClass = "";
            Scripts = new string[0];
            SourceDirectory = "";

            IsValid = false;
            ErrorMessage = "No valid plugin information is set.";
        }

        public string Id { get; set; }
        public string FriendlyName { get; set; }
        public Version PluginVersion { get; set; }
        public string[] Authors { get; set; }
        public string Description { get; set; }

        public string PluginType { get; set; }
        public string AssemblyPath { get; set; }
        public string PluginClass { get; set; }
        public string[] Scripts { get; set; }

        public string SourceDirectory { get; set; }

        /// <summary>
        /// Gets the validity of discovered information.
        /// </summary>
        public bool IsValid { get; set; }
        /// <summary>
        /// If <see cref="P:LauncherZLib.Plugin.PluginDiscoveryInfo.IsValid"/> is false,
        /// this property contains the error message.
        /// </summary>
        public string ErrorMessage { get; set; }
        
        public static PluginDiscoveryInfo FromType(Type t, bool reflectionOnly)
        {
            if (t == null)
                throw new ArgumentNullException("t");

            string path = GetTypeAssemblyPath(t);

            IList<CustomAttributeData> attrs = CustomAttributeData.GetCustomAttributes(t);
            if (attrs.Count == 0)
                return new PluginDiscoveryInfo();
            // find desired attributes
            CustomAttributeData pluginAttrData = null;
            CustomAttributeData descriptionAttrData = null;
            foreach (var attr in attrs)
            {
                if (reflectionOnly)
                {
                    if (attr.AttributeType == ReflOnlyPluginAttrType)
                    {
                        pluginAttrData = attr;
                    }
                    else if (attr.AttributeType == ReflOnlyDescriptionAttrType)
                    {
                        descriptionAttrData = attr;
                    }
                }
                else
                {
                    if (attr.AttributeType == typeof(PluginAttribute))
                    {
                        pluginAttrData = attr;
                    }
                    else if (attr.AttributeType == typeof(DescriptionAttribute))
                    {
                        descriptionAttrData = attr;
                    }
                }
            }
            if (pluginAttrData == null)
                return new PluginDiscoveryInfo();
            // init plugin discovery information
            var pdi = new PluginDiscoveryInfo
            {
                AssemblyPath = path,
                PluginType = Plugin.PluginType.Assembly,
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
                return new PluginDiscoveryInfo()
                {
                    ErrorMessage = ex.ToString()
                };
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
            // set validity
            pdi.IsValid = errMessages.Count == 0;
            pdi.ErrorMessage = string.Join(" ", errMessages);
            return pdi;
        }

        private static PluginAttribute CreatePluginAttributeFromData(CustomAttributeData attr)
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

        private static DescriptionAttribute CreateDescriptionAttributeFromData(CustomAttributeData attr)
        {
            var description = attr.ConstructorArguments[0].Value as string;
            return description == null ? DescriptionAttribute.Default : new DescriptionAttribute(description);
        }

        private static string GetTypeAssemblyPath(Type t)
        {
            string escapedCodeBase = t.Assembly.EscapedCodeBase;
            var uriBuilder = new UriBuilder(escapedCodeBase);
            return Uri.UnescapeDataString(uriBuilder.Path);
        }

    }
}
