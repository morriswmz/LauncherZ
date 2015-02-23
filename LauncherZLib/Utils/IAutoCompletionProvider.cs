using System.Collections.Generic;

namespace LauncherZLib.Utils
{
    public interface IAutoCompletionProvider
    {

        IEnumerable<string> GetAutoCompletions(string context, int limit);

    }
}
