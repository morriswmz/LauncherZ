using System;
using System.Windows.Input;

namespace LauncherZ
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execCallback;
        private readonly Predicate<object> _predicate;

        public RelayCommand(Action<object> execCallback) : this(execCallback, null)
        {
            
        }

        public RelayCommand(Action<object> execCallback, Predicate<object> predicate)
        {
            if (execCallback == null)
                throw new ArgumentNullException("execCallback");

            _execCallback = execCallback;
            _predicate = predicate;
        }

        public bool CanExecute(object parameter)
        {
            return _predicate == null || _predicate(parameter);
        }

        public void Execute(object parameter)
        {
            _execCallback(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

}
