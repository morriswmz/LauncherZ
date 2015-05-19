using System;
using LauncherZLib.Utils;

namespace LauncherZLib.Icon
{
    /// <summary>
    /// Describes the location of an icon.
    /// </summary>
    public sealed class IconLocation : IEquatable<IconLocation>
    {
        public static readonly IconLocation NotFound = new IconLocation();
        public static readonly string Separator = "://";

        private readonly string _domainInvariant;
        private string _fullString;

        /// <summary>
        /// <para>Domain of the icon resource. This property is case insensitive. </para>
        ////<para>By default, two domains "static" and "file" are defined.</para>
        /// </summary>
        /// <remarks>
        /// When comparing, domain value will be converted to uppercase using
        /// <see cref="M:System.String.ToUpperInvariant"/>.
        /// This behavior is consistent with
        /// <see cref="F:System.StringComparison.OrdinalIgnoreCase"/>.
        /// </remarks>
        public string Domain { get; private set; }
        
        /// <summary>
        /// <para>Path of the icon resource relative to the domain.</para>
        /// <para>This property is case sensitive.</para>
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Creates a new icon location.
        /// </summary>
        /// <param name="location"></param>
        /// <example>
        /// <code>
        /// var loc = new IconLocation("File://assets/myicon.png");
        /// string domain = loc.Domain; // "File"
        /// string path = loc.Path; // "assets/myicon.png"
        /// </code>
        /// </example>
        public IconLocation(string location)
        {
            if (string.IsNullOrEmpty(location))
                throw new ArgumentNullException("location");
            int idx = location.IndexOf(Separator, StringComparison.Ordinal);
            if (idx > 0)
            {
                Domain = location.Substring(0, idx);
                if (!Domain.IsProperDomainName())
                {
                    throw new ArgumentException(string.Format("{0} is not a valid domain name.", Domain));
                }
                Path = location.Substring(idx + Separator.Length);
            }
            else
            {
                throw new ArgumentException("Domain cannot be empty.");
            }
            
            Path = Path.Replace('\\', '/');
            _domainInvariant = Domain.ToUpperInvariant();
        }

        /// <summary>
        /// Specify icon location with domain and path.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="path"></param>
        public IconLocation(string domain, string path)
        {
            if (domain == null)
                throw new ArgumentNullException("domain");
            if (path == null)
                throw new ArgumentNullException("path");
            if (!domain.IsProperDomainName())
                throw new ArgumentException(string.Format("{0} is not a valid domain name.", Domain));

            Domain = domain;
            Path = path;
            _domainInvariant = Domain.ToUpperInvariant();
        }

        /// <summary>
        /// Special constructor for NotFound value. 
        /// </summary>
        private IconLocation()
        {
            Domain = "";
            Path = "";
        }

        public bool Equals(IconLocation other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(_domainInvariant, other._domainInvariant) && string.Equals(Path, other.Path);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IconLocation) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_domainInvariant != null ? _domainInvariant.GetHashCode() : 0)*397) ^ (Path != null ? Path.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return _fullString ?? (_fullString = string.Format("{0}{1}{2}", Domain, Separator, Path));
        }
    }
}
