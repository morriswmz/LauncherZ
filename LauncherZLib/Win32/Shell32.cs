using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.Win32
{
    public class Shell32
    {
        internal const int MAX_PATH = 260;

        // read more: http://msdn.microsoft.com/en-us/library/windows/desktop/bb762179(v=vs.85).aspx

        /// <summary>
        /// Version 5.0. Apply the appropriate overlays to the file's icon. The SHGFI_ICON flag
        /// must also be set.
        /// </summary>
        internal const uint SHGFI_ADDOVERLAYS = 0x000000020;
        /// <summary>
        /// Retrieve the handle to the icon that represents the file and the index of the icon
        /// within the system image list. The handle is copied to the hIcon member of the structure
        /// specified by psfi, and the index is copied to the iIcon member.
        /// </summary>
        internal const uint SHGFI_ICON = 0x000000100;
        /// <summary>
        /// Modify SHGFI_ICON, causing the function to retrieve the file's large icon.
        /// The SHGFI_ICON flag must also be set.
        /// </summary>
        internal const uint SHGFI_LARGEICON = 0x000000000;
        /// <summary>
        /// Modify SHGFI_ICON, causing the function to retrieve the file's small icon. Also used
        /// to modify SHGFI_SYSICONINDEX, causing the function to return the handle to the system
        /// image list that contains small icon images. The SHGFI_ICON and/or SHGFI_SYSICONINDEX
        /// flag must also be set.
        /// </summary>
        internal const uint SHGFI_SMALLICON = 0x000000001;
        /// <summary>
        /// Modify SHGFI_ICON, causing the function to add the link overlay to the file's icon.
        /// The SHGFI_ICON flag must also be set.
        /// </summary>
        internal const uint SHGFI_LINKOVERLAY = 0x000008000;
        /// <summary>
        /// Indicates that the function should not attempt to access the file specified by pszPath.
        /// Rather, it should act as if the file specified by pszPath exists with the file attributes
        /// passed in dwFileAttributes. This flag cannot be combined with the SHGFI_ATTRIBUTES,
        /// SHGFI_EXETYPE, or SHGFI_PIDL flags.
        /// </summary>
        internal const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;



        /// <summary>
        /// Retrieves information about an object in the file system, such as a file, folder,
        /// directory, or drive root.
        /// </summary>
        /// <param name="pszPath">
        /// <para>A pointer to a null-terminated string of maximum length MAX_PATH that contains
        /// the path and file name. Both absolute and relative paths are valid. </para>
        /// <para>If the uFlags parameter includes the SHGFI_PIDL flag, this parameter must be the
        /// address of an ITEMIDLIST (PIDL) structure that contains the list of item identifiers
        /// that uniquely identifies the file within the Shell's namespace. The PIDL must be a fully
        /// qualified PIDL. Relative PIDLs are not allowed.</para>
        /// <para>If the uFlags parameter includes the SHGFI_USEFILEATTRIBUTES flag, this parameter
        /// does not have to be a valid file name. The function will proceed as if the file exists
        /// with the specified name and with the file attributes passed in the dwFileAttributes
        /// parameter. This allows you to obtain information about a file type by passing just the
        /// extension for pszPath and passing FILE_ATTRIBUTE_NORMAL in dwFileAttributes.</para>
        /// <para>This string can use either short (the 8.3 form) or long file names.</para>
        /// </param>
        /// <param name="dwFileAttributes">A combination of one or more file attribute flags
        /// (FILE_ATTRIBUTE_ values as defined in Winnt.h). If uFlags does not include the
        /// SHGFI_USEFILEATTRIBUTES flag, this parameter is ignored.</param>
        /// <param name="psfi">Pointer to a SHFILEINFO structure to receive the file information.
        /// </param>
        /// <param name="cbSizeFileInfo">The size, in bytes, of the SHFILEINFO structure pointed to
        /// by the psfi parameter.</param>
        /// <param name="uFlags">The flags that specify the file information to retrieve.</param>
        /// <returns>Returns a value whose meaning depends on the uFlags parameter.</returns>
        [DllImport("shell32.dll")]
        internal static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
    }
    
}
