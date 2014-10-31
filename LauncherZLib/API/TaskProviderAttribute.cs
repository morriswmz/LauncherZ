using System;

namespace LauncherZLib.API
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TaskProviderAttribute : Attribute
    {

        private readonly string _idValue;

        public TaskProviderAttribute(string id)
        {
            _idValue = id;
            Name = "";
            Authors = "";
            Version = "";
        }

        public string Id { get { return _idValue; } }

        public string Version { get; set; }

        public string Name { get; set; }

        public string Authors { get; set; }
    }
}
