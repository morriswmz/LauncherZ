using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.API;

namespace LauncherZLib
{
    /// <summary>
    /// Manages user defined variables.
    /// </summary>
    public class UserVariableManager : IAutoCompletionProvider
    {


        public IEnumerable<string> GetAutoCompletions(string context, int limit)
        {
            throw new NotImplementedException();
        }
    }
}
