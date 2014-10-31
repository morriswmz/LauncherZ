using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZ.Icon
{
    public class IconLocation
    {
        public IconDomain Domain { get; private set; }
        public string Path { get; private set; }

        public IconLocation(string iconLocation)
        {
            if (string.IsNullOrEmpty(iconLocation))
            {
                Domain = IconDomain.Unknown;
                Path = "";
                return;
            }

            int idx = iconLocation.IndexOf(':');
            string domainStr;
            if (idx < 0)
            {
                // assume external
                domainStr = "external";
                Path = iconLocation;
            }
            else
            {
                domainStr = iconLocation.Substring(0, idx).ToLower();
                Path = iconLocation.Substring(idx + 1);
            }
            switch (domainStr)
            {
                case "internal":
                    Domain = IconDomain.Internal;
                    break;
                case "external":
                    Domain = IconDomain.External;
                    break;
                case "externalEmbedded":
                    Domain = IconDomain.ExternalEmbedded;
                    break;
                default:
                    Domain = IconDomain.Unknown;
                    break;
            }
        }

    }

    public enum IconDomain
    {
        /// <summary>
        /// Internal icons used by application, always available.
        /// </summary>
        Internal,
        /// <summary>
        /// External icons stored as image files.
        /// </summary>
        External,
        /// <summary>
        /// External icons that are embedded in non-image files.
        /// </summary>
        ExternalEmbedded,
        /// <summary>
        /// Unkown. Cannot be resolved.
        /// </summary>
        Unknown
    }
}
