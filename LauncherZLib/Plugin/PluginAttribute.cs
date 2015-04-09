using System;

namespace LauncherZLib.Plugin
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class PluginAttribute : Attribute
    {

        private const string DefaultVersion = "0.0.0.0";

        private readonly string _id;
        private string _friendlyName = "";
        private string _version = DefaultVersion;
        private string _authors = "";

        public PluginAttribute(string id)
        {
            _id = id ?? "";
        }

        /// <summary>
        /// Gets the plugin id.
        /// </summary>
        public string Id { get { return _id; } }

        /// <summary>
        /// Gets or sets the friendly name.
        /// If given value is empty or white space, plugin id will be returned.
        /// </summary>
        public string FriendlyName
        {
            get { return _friendlyName; }
            set { _friendlyName = string.IsNullOrWhiteSpace(value) ? _id : value; }
        }

        /// <summary>
        /// Gets or sets the authors.
        /// If no authors are set, empty string will be returned.
        /// </summary>
        public string Authors
        {
            get { return _authors; }
            set { _authors = value ?? ""; }
        }

        /// <summary>
        /// Gets or sets the plugin version.
        /// If given value is invalid, "0.0.0.0" will be used.
        /// </summary>
        public string Version
        {
            get { return _version; }
            set { _version = value ?? DefaultVersion; }
        }


    }
}
