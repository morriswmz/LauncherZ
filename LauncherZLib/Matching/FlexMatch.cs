﻿using System;

namespace LauncherZLib.Matching
{
    /// <summary>
    /// Represents a match.
    /// </summary>
    // todo FlexMatch depends on the context, overriding Equals may be inappropriate
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

        protected bool Equals(FlexMatch other)
        {
            return StartIndex == other.StartIndex && Length == other.Length && string.Equals(Content, other.Content);
        }

        public override string ToString()
        {
            return string.Format("{{StartIndex={0}, Length={1}, Content={2}}}", StartIndex, Length, Content);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FlexMatch) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = StartIndex;
                hashCode = (hashCode*397) ^ Length;
                hashCode = (hashCode*397) ^ (Content != null ? Content.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}