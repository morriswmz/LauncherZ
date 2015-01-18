using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.Matching
{

    /// <summary>
    /// A simple read-only collection implementation for <see cref="T:LauncherZLib.Matching.FlexMatch"/>.
    /// </summary>
    public class FlexMatchCollection : IReadOnlyCollection<FlexMatch>
    {

        public static readonly FlexMatchCollection Empty = new FlexMatchCollection(new FlexMatch[0]);

        private readonly FlexMatch[] _matches;

        public FlexMatchCollection(FlexMatch[] matches)
        {
            _matches = matches;
            Count = _matches.Length;
        }

        public FlexMatch this[int index]
        {
            get { return _matches[index]; }
        }

        public int Count { get; private set; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _matches.GetEnumerator();
        }

        public IEnumerator<FlexMatch> GetEnumerator()
        {
            return _matches.AsEnumerable().GetEnumerator();
        }

    }
}
