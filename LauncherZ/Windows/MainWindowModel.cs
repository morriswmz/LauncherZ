﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LauncherZ.Controls;
using LauncherZLib.Launcher;

namespace LauncherZ.Windows
{
    internal sealed class MainWindowModel : INotifyPropertyChanged
    {

        private string _inputText = "";
        private SelectionRange _inputSelectionRange = new SelectionRange(0, 0);
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
        
        private void RaisePropertyChangedEvent([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
