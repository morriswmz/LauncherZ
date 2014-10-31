using System;

namespace LauncherZLib.Task.Provider
{
    /// <summary>
    /// Stores type information of a task provider.
    /// Type information is retrieved in discovering phase and used for
    /// loading providers later.
    /// </summary>
    [Serializable]
    internal sealed class TaskProviderTypeInfo
    {
        /// <summary>
        /// Full name of the type.
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Location of the assembly containing this type.
        /// </summary>
        public string AssemblyLocation { get; set; }

        /// <summary>
        /// Id of the associated task provider.
        /// Read from <see cref="LauncherZLib.API.TaskProviderAttribute"/>.
        /// </summary>
        public string ProviderId { get; set; }

        /// <summary>
        /// Name of the associated task provider.
        /// Read from <see cref="LauncherZLib.API.TaskProviderAttribute"/>
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Authors of the associated task provider.
        /// Read from <see cref="LauncherZLib.API.TaskProviderAttribute"/>
        /// </summary>
        public string[] Authors { get; set; }

        /// <summary>
        /// Description of the associated task provider.
        /// Read from <see cref="System.ComponentModel.DescriptionAttribute"/>
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Priority of the associated task provider.
        /// Read from <see cref="LauncherZLib.API.PriorityAttribute"/>
        /// </summary>
        public double Priority { get; set; }

    }
}
