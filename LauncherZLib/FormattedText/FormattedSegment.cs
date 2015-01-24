using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.FormattedText
{
    public sealed class FormattedSegment
    {

        public TextFormat Format { get; private set; }
        public string Text { get; private set; }

        public FormattedSegment(string text, TextFormat format)
        {
            Text = text;
            Format = format;
        }

    }
}
