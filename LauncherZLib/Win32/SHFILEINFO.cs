using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.Win32
{
    /// <summary>
    /// Contains information about a file object.
    /// Read more: http://msdn.microsoft.com/en-us/library/windows/desktop/bb759792(v=vs.85).aspx
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct SHFILEINFO
    {
        /// <summary>
        /// A handle to the icon that represents the file. You are responsible for destroying this
        /// handle with DestroyIcon when you no longer need it.
        /// </summary>
        public IntPtr hIcon;
        /// <summary>
        /// The index of the icon image within the system image list.
        /// </summary>
        public int iIcon;
        /// <summary>
        /// An array of values that indicates the attributes of the file object. For information
        /// about these values, see the IShellFolder::GetAttributesOf method.
        /// </summary>
        public uint dwAttributes;
        /// <summary>
        /// A string that contains the name of the file as it appears in the Windows Shell, or
        /// the path and file name of the file that contains the icon representing the file.
        /// 260 is the number of MAX_PATH
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        /// <summary>
        /// A string that describes the type of file.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };
}
