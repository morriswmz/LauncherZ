using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.Win32
{
    public class User32
    {
        /// <summary>
        /// Destroys an icon and frees any memory the icon occupied.
        /// </summary>
        /// <param name="hIcon">A handle to the icon to be destroyed. The icon must not be in use.</param>
        /// <returns>
        /// <para>If the function succeeds, the return value is nonzero.</para>
        /// <para>If the function fails, the return value is zero. To get extended error information,
        /// call GetLastError.</para>
        /// </returns>
        /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/ms648063%28v=vs.85%29.aspx"/>
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool DestroyIcon(IntPtr hIcon);


        /// <summary>
        /// Alt key modifier.
        /// </summary>
        internal const uint MOD_ALT = 0x0001;
        /// <summary>
        /// Control key modifier.
        /// </summary>
        internal const uint MOD_CONTROL = 0x0002;
        /// <summary>
        /// Shift key modifier.
        /// </summary>
        internal const uint MOD_SHIFT = 0x0004;
        /// <summary>
        /// Win key modifier.
        /// </summary>
        internal const uint MOD_WIN = 0x0008;
        /// <summary>
        /// Changes the hotkey behavior so that the keyboard auto-repeat does not yield multiple
        /// hotkey notifications.
        /// Windows Vista and Windows XP/2000:  This flag is not supported.
        /// </summary>
        internal const uint MOD_NOREPEAT = 0x4000;

        /// <summary>
        /// Posted when the user presses a hot key registered by the RegisterHotKey function.
        /// The message is placed at the top of the message queue associated with the thread that
        /// registered the hot key.
        /// </summary>
        /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/ms646279%28v=vs.85%29.aspx"/>
        internal const int WM_HOTKEY = 0x312;

        /// <summary>
        /// Defines a system-wide hot key.
        /// </summary>
        /// <param name="hwnd">A handle to the window that will receive WM_HOTKEY messages generated
        /// by the hot key. If this parameter is NULL, WM_HOTKEY messages are posted to the message
        /// queue of the calling thread and must be processed in the message loop.</param>
        /// <param name="id">The identifier of the hot key. If the hWnd parameter is NULL, then the
        /// hot key is associated with the current thread rather than with a particular window. If a
        /// hot key already exists with the same hWnd and id parameters, see Remarks for the action
        /// taken.</param>
        /// <param name="fsModifiers">The keys that must be pressed in combination with the key
        /// specified by the uVirtKey parameter in order to generate the WM_HOTKEY message. The
        /// fsModifiers parameter can be a combination of the following values.</param>
        /// <param name="vk">The virtual-key code of the hot key. See Virtual Key Codes.</param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero. To get extended error information,
        /// call GetLastError.
        /// </returns>
        /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/ms646309(v=vs.85).aspx"/>
        [DllImport("user32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool RegisterHotKey(IntPtr hwnd, int id, uint fsModifiers, uint vk);

        /// <summary>
        /// Frees a hot key previously registered by the calling thread.
        /// </summary>
        /// <param name="hwnd">A handle to the window associated with the hot key to be freed. This
        /// parameter should be NULL if the hot key is not associated with a window.</param>
        /// <param name="id">The identifier of the hot key to be freed.</param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero. To get extended error information,
        /// call GetLastError.
        /// </returns>
        /// <see cref="http://msdn.microsoft.com/en-us/library/windows/desktop/ms646327(v=vs.85).aspx"/>
        [DllImport("user32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnregisterHotKey(IntPtr hwnd, int id);
    }
}
