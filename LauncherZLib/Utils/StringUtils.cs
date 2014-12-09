using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LauncherZLib.Utils
{
    public static class StringUtils
    {
        public static readonly Regex ProperIdPattern = new Regex(@"^[_a-z][_a-z0-9]*$", RegexOptions.IgnoreCase);

        public static bool IsProperId(this string str)
        {
            return ProperIdPattern.IsMatch(str);
        }
    }
}
