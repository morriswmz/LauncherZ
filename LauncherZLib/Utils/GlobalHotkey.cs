using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using LauncherZLib.Win32;

namespace LauncherZLib.Utils
{

    /// <summary>
    /// Helps define and register a global hotkey.
    /// </summary>
    public class GlobalHotkey : IDisposable
    {

        private bool _disposed = false;
        private int _id = -1;
        private IntPtr _hWnd;
        private readonly Key[] _keys;
        private readonly Key _hotkey;
        private readonly uint _modifiers;
        private HwndSource _hSource;
        
        /// <summary>
        /// Initialize a global hotkey with specified key combination.
        /// </summary>
        /// <param name="keyCombo">A valid key combination. e.g. Win+OemQuestion</param>
        public GlobalHotkey(string keyCombo)
        {
            IsRegistered = false;
            Key[] keys = KeyboardUtils.ParseKeyCombination(keyCombo);
            uint modifiers = 0x0000;
            var hotkey = Key.None;
            foreach (var key in keys)
            {
                if (key.Equals(Key.None))
                    throw new ArgumentException("Key cannot be none.");

                if (key.Equals(Key.LeftCtrl) || key.Equals(Key.RightCtrl))
                {
                    if ((modifiers & User32.MOD_CONTROL) != 0)
                        throw new ArgumentException("Ctrl key appeared twice in the key combination.");
                    modifiers |= User32.MOD_CONTROL;
                }
                else if (key.Equals(Key.LeftAlt) || key.Equals(Key.RightAlt))
                {
                    if ((modifiers & User32.MOD_ALT) != 0)
                        throw new ArgumentException("Alt key appeared twice in the key combination.");
                    modifiers |= User32.MOD_ALT;
                }
                else if (key.Equals(Key.LeftShift) || key.Equals(Key.RightShift))
                {
                    if ((modifiers & User32.MOD_SHIFT) != 0)
                        throw new ArgumentException("Shift key appeared twice in the key combination.");
                    modifiers |= User32.MOD_SHIFT;
                }
                else if (key.Equals(Key.LWin) || key.Equals(Key.RWin))
                {
                    if ((modifiers & User32.MOD_WIN) != 0)
                        throw new ArgumentException("Win key appeared twice in the key combination.");
                    modifiers |= User32.MOD_WIN;
                }
                else
                {
                    if (!hotkey.Equals(Key.None))
                        throw new ArgumentException(string.Format("Hotkey is already assign as {0}.", hotkey));
                    hotkey = key;
                }
            }
            _hotkey = hotkey;
            _modifiers = modifiers;
            _keys = keys;
        }

        ~GlobalHotkey()
        {
            Dispose(false);
        }

        /// <summary>
        /// Occurs when registered global hotkey is pressed.
        /// </summary>
        public event EventHandler HotkeyPressed;

        /// <summary>
        /// Gets the id associated with the global hotkey.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="T:System.InvalidOperationException">
        /// Thrown when the global hotkey is unregistered.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        /// Thrown when the object is disposed.
        /// </exception>
        public int GetId()
        {
            VerifyStatus(true);
            return _id;
        }

        /// <summary>
        /// Gets the window handle used in registeration.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="T:System.InvalidOperationException">
        /// Thrown when the global hotkey is unregistered.
        /// </exception>
        /// <exception cref="T:System.ObjectDisposedException">
        /// Thrown when the object is disposed.
        /// </exception>
        public IntPtr GetHandle()
        {
            VerifyStatus(true);
            return _hWnd;
        }

        /// <summary>
        /// Gets the key combination as an array.
        /// </summary>
        /// <returns></returns>
        public Key[] GetKeyCombination()
        {
            return (Key[])_keys.Clone();
        }

        /// <summary>
        /// Gets whether the global hotkey is registered.
        /// </summary>
        public bool IsRegistered { get; private set; }

        /// <summary>
        /// Gets the hotkey in the key combination.
        /// </summary>
        public Key Hotkey { get { return _hotkey; } }

        /// <summary>
        /// Gets whether the global hotkey has control key as modifier.
        /// </summary>
        public bool HasControlModifier { get { return (_modifiers & User32.MOD_CONTROL) > 0; } }

        /// <summary>
        /// Gets whether the global hotkey has alt key as modifier.
        /// </summary>
        public bool HasAltModifier { get { return (_modifiers & User32.MOD_ALT) > 0; } }

        /// <summary>
        /// Gets whether the global hotkey has shift key as modifier.
        /// </summary>
        public bool HasShiftModifier { get { return (_modifiers & User32.MOD_SHIFT) > 0; } }

        /// <summary>
        /// Gets whether the global hotkey has win key as modifier.
        /// </summary>
        public bool HasWinModifier { get { return (_modifiers & User32.MOD_WIN) > 0; } }

        /// <summary>
        /// Registers the global hotkey.
        /// If the global hotkey is already registered, <see cref="T:System.InvalidOperationException"/>
        /// will be thrown.
        /// </summary>
        /// <param name="window">Associated window to receive WM_HOTKEY.</param>
        /// <param name="id">Associated id for the hotkey. With the same window, this value must be unique
        /// for different global hotkeys.</param>
        /// <returns></returns>
        public GlobalHotkey Register(Window window, int id)
        {
            VerifyStatus(false);
            _id = id;
            var winIterop = new WindowInteropHelper(window);
            winIterop.EnsureHandle();
            _hWnd = winIterop.Handle;
            _hSource = HwndSource.FromHwnd(_hWnd);
            if (_hSource == null)
            {
                throw new Exception("Failed to create HwndSource.");
            }
            _hSource.AddHook(HotkeyHook);
            if (!User32.RegisterHotKey(_hWnd, _id, _modifiers, (uint) KeyInterop.VirtualKeyFromKey(_hotkey)))
            {
                int errCode = Marshal.GetLastWin32Error();
                _hSource.RemoveHook(HotkeyHook);
                _hSource.Dispose();
                throw new Win32Exception(errCode);
            }
            IsRegistered = true;
            return this;
        }

        /// <summary>
        /// Unregisters the global hotkey.
        /// If the global hotkey is already unregistered, <see cref="T:System.InvalidOperationException"/>
        /// will be thrown.
        /// </summary>
        /// <returns></returns>
        public GlobalHotkey Unregister()
        {
            VerifyStatus(true);
            _hSource.RemoveHook(HotkeyHook);
            _hSource.Dispose();
            if (!User32.UnregisterHotKey(_hWnd, _id))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            IsRegistered = false;
            return this;
        }
        
        /// <summary>
        /// Disposes the object.
        /// Global hotkey will be unregistered and unmanaged resources will be released.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            // dispose managed resources
            if (disposing)
            {
                if (_hSource != null)
                {
                    _hSource.RemoveHook(HotkeyHook);
                    _hSource.Dispose();
                }
            }

            // dispose unmanaged resources
            if (IsRegistered)
            {
                IsRegistered = false;
                User32.UnregisterHotKey(_hWnd, _id);
            }

            _disposed = true;
        }

        private void VerifyStatus(bool expectRegistered)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName);
            if (!IsRegistered && expectRegistered)
                throw new InvalidOperationException("Hotkey is not registered.");
            if (IsRegistered && !expectRegistered)
                throw new InvalidOperationException("Hotkey is already registered.");
        }

        private IntPtr HotkeyHook(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == User32.WM_HOTKEY && wparam.ToInt32() == _id)
            {
                var handlers = HotkeyPressed;
                if (handlers != null)
                {
                    handlers(this, new EventArgs());
                }
                handled = true;
            }
            return IntPtr.Zero;
        }


    }
}
