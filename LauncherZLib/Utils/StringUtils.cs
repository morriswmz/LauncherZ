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
        public static readonly Regex ProperIdPattern = new Regex(@"^[_a-z][_a-z0-9]*(\.[_a-z][_a-z0-9]*)*$",
            RegexOptions.IgnoreCase);

        public static bool IsProperId(this string str)
        {
            return ProperIdPattern.IsMatch(str);
        }

        public static string GetSlashEscapedChar(char charAfterSlash)
        {
            switch (charAfterSlash)
            {
                case '\'':
                    return "'";
                case '"':
                    return "\"";
                case '\\':
                    return "\\";
                case '0':
                    return "\u0000";
                case 'a':
                    return "\u0007";
                case 'b':
                    return "\u0008";
                case 'f':
                    return "\u000C";
                case 'n':
                    return "\u000A";
                case 'r':
                    return "\u000D";
                case 't':
                    return "\u0009";
                case 'v':
                    return "\u000B";
                default:
                    return "";
            }
        }
    }


}
