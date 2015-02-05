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

        /// <summary>
        /// Creates a progress bar using text symbols.
        /// </summary>
        /// <param name="template">
        /// A template of length 4. For example, "[=-]" will produce a progress bar like "[===------]".
        /// </param>
        /// <param name="length">Length of the progress bar, excluding both ends.</param>
        /// <param name="percentage">A double value between 0.0 and 1.0.</param>
        /// <returns></returns>
        public static string CreateProgressBar(string template, int length, double percentage)
        {
            if (string.IsNullOrEmpty(template) || template.Length != 4)
                throw new ArgumentException("Invalid template.", "template");
            if (length < 1)
                throw new ArgumentOutOfRangeException("length", "Length must be positive.");
            if (double.IsNaN(percentage))
                throw new ArgumentException("Percentage cannot be NaN.", "percentage");

            if (percentage > 1.0) percentage = 1.0;
            if (percentage < 0.0) percentage = 0.0;
            var n = (int) Math.Round(length*percentage);
            return string.Format("{0}{1}{2}{3}", template[0], new string(template[1], n), new string(template[2], length - n), template[3]);
        }
    }


}
