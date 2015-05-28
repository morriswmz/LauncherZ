using System;

namespace LauncherZLib.Matching
{
    /// <summary>
    /// Represents a match.
    /// </summary>
    public class FlexMatch
    {

        /// <summary>
        /// Gets the starting index of the match.
        /// </summary>
        public int StartIndex { get; private set; }

        /// <summary>
        /// Gets the length of the match.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Gets the matched content.
        /// </summary>
        public string Content { get; private set; }

        public FlexMatch(int startIndex, int length, string content)
        {
            StartIndex = startIndex;
            Length = length;
            Content = content;
        }

        /// <summary>
        /// Checks overlap.
        /// </summary>
        /// <param name="otherStartIdx"></param>
        /// <param name="otherLength"></param>
        /// <returns></returns>
        public bool OverlapsWith(int otherStartIdx, int otherLength)
        {
            int left = Math.Min(StartIndex, otherStartIdx);
            int right = Math.Max(StartIndex + Length, otherStartIdx + otherLength);
            return (right - left) < (otherLength + Length);
        }

        public override string ToString()
        {
            return string.Format("{{StartIndex={0}, Length={1}, Content={2}}}", StartIndex, Length, Content);
        }

    }
}