using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.Matching
{
    /// <summary>
    /// Gives <see cref="T:LauncherZLib.Matching.FlexMatchResult"/> a score.
    /// </summary>
    public class FlexScorer
    {

        private double _exactWeight = 1.0;
        private double _flexWeight = 1.0;
        private double _exactDelay = 1.0;
        private double _flexDelay = 1.0;

        public double Score(string originalString, FlexMatchResult result)
        {
            double exactScore = 0.0;
            foreach (var exactMatch in result.ExactMatches)
            {
                exactScore += 1.0/(1.0 + _exactDelay * exactMatch.StartIndex);
            }
            if (result.ExactMatches.Count > 0)
                exactScore /= result.ExactMatches.Count;

            double flexScore = 0.0;
            foreach (var flexMatch in result.FlexMatches)
            {
                flexScore += 1.0 / (1.0 + _flexDelay * flexMatch.StartIndex);
            }
            if (result.FlexMatches.Count > 0)
                flexScore /= result.FlexMatches.Count;
            
            return (_exactWeight * exactScore + _flexWeight * flexScore) / (_exactWeight + _flexWeight);
        }

    }
}
