using System;

namespace LauncherZ.Plugin
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginAttribute : Attribute
    {

        public PluginAttribute(string id, string name, string authors, string version)
        {
            Id = id;
            Name = name;
            Authors = authors;
            Version = version;
        }

        public string Id { get; private set; }

        public string Version { get; private set; }

        public string Name { get; private set; }

        public string Authors { get; private set; }
    }
}
