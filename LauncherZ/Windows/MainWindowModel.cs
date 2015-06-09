using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LauncherZ.Controls;
using LauncherZLib.Launcher;

namespace LauncherZ.Windows
{
    internal sealed class MainWindowModel : INotifyPropertyChanged
    {

        private string _inputText = "";
        private string _hintText = "LauncherZ";
        private bool _isInputEnabled = true;
        private SelectionRange _inputSelectionRange = new SelectionRange(0, 0);
        private ReadOnlyObservableCollection<LauncherData> _launchers;
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

        public string HintText
        {
            get { return _hintText; }
            set
            {
                if (_hintText != value)
                {
                    _hintText = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public bool IsInputEnabled
        {
            get { return _isInputEnabled; }
            set
            {
                if (_isInputEnabled != value)
                {
                    _isInputEnabled = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public SelectionRange InputSelectionRange
        {
            get { return _inputSelectionRange; }
            set
            {
                if (_inputSelectionRange != value)
                {
                    _inputSelectionRange = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public ReadOnlyObservableCollection<LauncherData> Launchers
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
        
        private void RaisePropertyChangedEvent([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
