using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CorePlugins.UnitConverter
{
    public sealed class Unit
    {
        private static readonly Regex InvalidCharPattern = new Regex(@"[.\s]");
        private static readonly Dictionary<string, Unit> ExistingUnits = new Dictionary<string, Unit>(); 
        
        private readonly string _fullName;
        private string[] _segments;

        /// <summary>
        /// Creates a unit from full unit name.
        /// </summary>
        /// <param name="fullName">
        /// Full unit name of format "dimension[.standard].name".
        /// </param>
        public static Unit FromFullName(string fullName)
        {
            if (fullName == null)
                throw new ArgumentNullException("fullName");

            Unit unit;
            if (ExistingUnits.TryGetValue(fullName, out unit))
            {
                return unit;
            }
            unit = new Unit(fullName);
            ExistingUnits.Add(fullName, unit);
            return unit;
        }

        public static bool IsValidFullName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return false;
            }
            int dotCount = 0;
            for (var i = 0; i < fullName.Length; i++)
            {
                if (char.IsWhiteSpace(fullName[i]))
                {
                    return false;
                }
                if (fullName[i] == '.')
                {
                    dotCount++;
                    if (dotCount > 3)
                    {
                        return false;
                    }
                }
            }
            return dotCount > 0;
        }

        /// <summary>
        /// Internal constructor.
        /// Assuming input parameter is checked and valid.
        /// </summary>
        /// <param name="fullName"></param>
        private Unit(string fullName)
        {
            _fullName = fullName;

            string[] splits = _fullName.Split('.');
            if (splits.Length < 2)
                throw new ArgumentException("Full unit name should at least contain dimension segment and actual name segment.");
            if (splits.Length > 3)
                throw new ArgumentException("Too many segments in specified full unit name.");

            if (splits.Length == 2)
            {
                VerifyAndSetSegments(splits[0], null, splits[1]);
            }
            else
            {
                VerifyAndSetSegments(splits[0], splits[1], splits[2]);
            }
        }

        public string FullName { get { return _fullName; } }

        public string Dimension { get { return _segments[0]; } }

        public string StandardPrefix { get { return _segments[1]; } }

        public string Name { get { return _segments[2]; } }

        /// <summary>
        /// Internel method for setting each segment.
        /// All arguments should not be null except standard.
        /// </summary>
        /// <param name="dimension"></param>
        /// <param name="standard"></param>
        /// <param name="name"></param>
        private void VerifyAndSetSegments(string dimension, string standard, string name)
        {
            if (InvalidCharPattern.IsMatch(dimension))
                throw new ArgumentException("Dimension name cannot contain dots or white spaces.");
            if (standard != null && InvalidCharPattern.IsMatch(standard))
                throw new ArgumentException("Standard prefix cannot contain dots or white spaces.");
            if (InvalidCharPattern.IsMatch(name))
                throw new ArgumentException("Name cannot contain dots or white spaces.");
            _segments = new string[4];
            _segments[0] = dimension;
            _segments[1] = standard ?? "";
            _segments[2] = name;
        }

        private bool Equals(Unit other)
        {
            return string.Equals(_fullName, other._fullName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Unit && Equals((Unit) obj);
        }

        public override int GetHashCode()
        {
            return _fullName.GetHashCode();
        }

        public override string ToString()
        {
            return _fullName;
        }
    }
}
