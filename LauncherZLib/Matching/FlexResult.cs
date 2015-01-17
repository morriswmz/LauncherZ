using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.Matching
{
    public class FlexResult
    {
        public bool IsExactMatchPerformed { get; private set; }

        public IEnumerable<FlexMatch> ExactMatches { get; private set; }

        public bool IsFlexMatchPerformed { get; private set; }

        public IEnumerable<FlexMatch> FlexMatches { get; private set; }

        public FlexResult(bool isExactMatchPerformed, IEnumerable<FlexMatch> exactMatches, bool isFlexMatchPerformed, IEnumerable<FlexMatch> flexMatches)
        {
            IsExactMatchPerformed = isExactMatchPerformed;
            ExactMatches = exactMatches;
            IsFlexMatchPerformed = isFlexMatchPerformed;
            FlexMatches = flexMatches;
        }
    }
}
