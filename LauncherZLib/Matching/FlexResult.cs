using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.Matching
{
    public class FlexResult
    {
        public FlexResult(bool isExactMatchPerformed, FlexMatchCollection exactMatches, bool isFlexMatchPerformed, FlexMatchCollection flexMatches)
        {
            Success = (isExactMatchPerformed && exactMatches.Count > 0) ||
                      (isFlexMatchPerformed && flexMatches.Count > 0);
            IsExactMatchPerformed = isExactMatchPerformed;
            ExactMatches = exactMatches ?? FlexMatchCollection.Empty;
            IsFlexMatchPerformed = isFlexMatchPerformed;
            FlexMatches = flexMatches ?? FlexMatchCollection.Empty;
        }

        public bool Success { get; private set; }

        public bool IsExactMatchPerformed { get; private set; }

        public FlexMatchCollection ExactMatches { get; private set; }

        public bool IsFlexMatchPerformed { get; private set; }

        public FlexMatchCollection FlexMatches { get; private set; }

    }
}
