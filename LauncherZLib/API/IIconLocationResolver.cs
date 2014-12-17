using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.Icon;

namespace LauncherZLib.API
{
    public interface IIconLocationResolver
    {
        bool TryResolve(IconLocation location, out string path);
    }
}
