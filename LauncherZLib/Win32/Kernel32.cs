using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.Win32
{
    public class Kernel32
    {
        /// <summary>
        /// Closes an open object handle.
        /// </summary>
        /// <param name="hObject"></param>
        /// <returns>
        /// <para>If the function succeeds, the return value is nonzero.</para>
        /// <para>If the function fails, the return value is zero. To get extended
        /// error information, call GetLastError.</para>
        /// </returns>
        /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/ms724211%28v=vs.85%29.aspx"/>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        /// <summary>
        /// Sets the last-error code for the calling thread.
        /// </summary>
        /// <param name="dwErrCode">The last-error code for the thread.</param>
        /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/ms680627(v=vs.85).aspx"/>
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern void SetLastError(uint dwErrCode);

        /// <summary>
        /// Adds a character string to the global atom table and returns a unique value (an atom)
        /// identifying the string.
        /// </summary>
        /// <param name="lpString">The null-terminated string to be added. The string can have a
        /// maximum size of 255 bytes. Strings that differ only in case are considered identical.
        /// The case of the first string of this name added to the table is preserved and returned
        /// by the GlobalGetAtomName function.</param>
        /// <returns>
        /// If the function succeeds, the return value is the newly created atom.
        /// If the function fails, the return value is zero. To get extended error information,
        /// call GetLastError.
        /// </returns>
        /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/ms649060%28v=vs.85%29.aspx"/>
        [DllImport("kernel32", SetLastError = true)]
        public static extern short GlobalAddAtom(string lpString);

        /// <summary>
        /// Decrements the reference count of a global string atom. If the atom's reference count
        /// reaches zero, GlobalDeleteAtom removes the string associated with the atom from the
        /// global atom table.
        /// </summary>
        /// <param name="nAtom">The atom and character string to be deleted.</param>
        /// <returns>
        /// <para>The function always returns (ATOM) 0.</para>
        /// <para>To determine whether the function has failed, call SetLastError with ERROR_SUCCESS
        /// before calling GlobalDeleteAtom, then call GetLastError. If the last error code is still
        /// ERROR_SUCCESS, GlobalDeleteAtom has succeeded.</para>
        /// </returns>
        /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/ms649061(v=vs.85).aspx"/>
        [DllImport("kernel32", SetLastError = true)]
        public static extern short GlobalDeleteAtom(short nAtom);
    }
}
