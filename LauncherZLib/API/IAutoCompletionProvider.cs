using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.API
{
    public interface IAutoCompletionProvider
    {

        IEnumerable<string> GetAutoCompletions(string context, int limit);

    }
}
