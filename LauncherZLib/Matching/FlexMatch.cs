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

    }
}