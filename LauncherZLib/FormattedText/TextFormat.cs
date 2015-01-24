using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.FormattedText
{
    [Flags]
    public enum TextFormat
    {
        Normal = 0,
        Bold = 1,
        Italic = 1 << 1,
        Underline = 1 << 2,
        NewLine = 1 << 8
    }
}
