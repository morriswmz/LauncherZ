using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LauncherZLib.Launcher;

namespace LauncherZ.Controls
{
    public class LauncherSelectionChangedEventArgs : RoutedEventArgs
    {

        private readonly LauncherData _oldItem;
        private readonly LauncherData _newItem;

        public LauncherData OldItem { get { return _oldItem; } }
        public LauncherData NewItem { get { return _newItem; } }

        public LauncherSelectionChangedEventArgs(RoutedEvent routedEvent, object source, LauncherData oldItem, LauncherData newItem)
            : base(routedEvent, source)
        {
            _oldItem = oldItem;
            _newItem = newItem;
        }
    }
}
