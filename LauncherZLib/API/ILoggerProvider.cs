using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.API
{
    /// <summary>
    /// Describes a logger provider.
    /// </summary>
    public interface ILoggerProvider
    {
        /// <summary>
        /// Creates a logger with specific category.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        ILogger CreateLogger(string category);
    }
}
