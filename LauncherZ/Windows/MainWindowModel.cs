using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LauncherZLib.Launcher;

namespace LauncherZ.Windows
{
    internal sealed class MainWindowModel : INotifyPropertyChanged
    {

        private string _inputText = "";
        private LauncherList _launchers;
        private LauncherData _selectedLauncher;
        private int _selectedIndex = -1;

        public event PropertyChangedEventHandler PropertyChanged;

        public string InputText
        {
            get { return _inputText; }
            set
            {
                if (_inputText != value)
                {
                    _inputText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public LauncherList Launchers
        {
            get { return _launchers; }
            set
            {
                if (_launchers != value)
                {
                    _launchers = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public LauncherData SelectedLauncher
        {
            get { return _selectedLauncher; }
            set
            {
                if (_selectedLauncher != value)
                {
                    _selectedLauncher = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex;  }
            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    RaisePropertyChangedEvent();
                }
            }
        }
        
        private void RaisePropertyChangedEvent([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
