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

        private bool Equals(FormattedSegment other)
        {
            return string.Equals(Text, other.Text) && Format == other.Format;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is FormattedSegment && Equals((FormattedSegment) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Text != null ? Text.GetHashCode() : 0)*397) ^ (int) Format;
            }
        }
    }
}
