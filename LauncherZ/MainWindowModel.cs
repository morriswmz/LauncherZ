using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using LauncherZLib.Launcher;

namespace LauncherZ
{
    internal sealed class MainWindowModel : INotifyPropertyChanged
    {

        private Visibility _windowVisibility = Visibility.Visible;
        private string _inputText = "";
        private LauncherList _launchers;
        private LauncherData _selectedLauncher;

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

        public Visibility WindowVisibility
        {
            get { return _windowVisibility; }
            set
            {
                if (_windowVisibility != value)
                {
                    _windowVisibility = value;
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
