using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.Icon
{

    public class IconLocation : IEquatable<IconLocation>
    {

        public static readonly char Separator = '|';

        /// <summary>
        /// Domain of the icon resource. Usually the provider id (e.g. LauncherZ).
        /// Note: Case sensitive.
        /// </summary>
        public string Domain { get; private set; }
        
        /// <summary>
        /// Path of the icon resource relative to the domain.
        /// If domain is null or empty, it should be the absolute path.
        /// e.g. "LauncherZ|Icons/default.png" will be expaned to
        ///     [FolderOfLauncherZAssembly]\Icons\default.png
        /// Note: Forward and backslash are interchangable.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Creates a new icon location.
        /// </summary>
        /// <param name="location"></param>
        public IconLocation(string location)
        {
            if (string.IsNullOrEmpty(location))
                throw new ArgumentNullException("location");
            int idx = location.IndexOf(Separator);
            if (idx >= 0)
            {
                Domain = location.Substring(0, idx);
                Path = location.Substring(idx + 1);
            }
            else
            {
                Domain = "";
                Path = location;
            }
            Path = Path.Replace('/', '\\');
        }

        /// <summary>
        /// Specify icon location with domain and path.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="path"></param>
        public IconLocation(string domain, string path)
        {
            Domain = domain;
            Path = path;
        }

        public bool Equals(IconLocation other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Domain, other.Domain) && string.Equals(Path, other.Path);
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
                return ((Domain != null ? Domain.GetHashCode() : 0)*397) ^ (Path != null ? Path.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Domain) ? Path : string.Format("{0}{1}{2}", Domain, Separator, Path);
        }
    }
}
