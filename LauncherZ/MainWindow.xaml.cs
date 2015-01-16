using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using LauncherZ.Controls;
using LauncherZLib;
using LauncherZLib.Event;
using LauncherZLib.Launcher;
using LauncherZLib.Utils;

namespace LauncherZ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        private DetailWindow detailWindow;
        private QueryController _queryController = new QueryController(LauncherZApp.Instance.PluginManager, 20);
        private GlobalHotkey _switchHotkey = new GlobalHotkey("Win+OemQuestion");

        private List<Key> _pressedKeys = new List<Key>();
        private bool _activating = false;
        private bool _deactivating = false;

        public MainWindow()
        {
            InitializeComponent();
            // register global hotkey
            _switchHotkey.Register(this, 0);
            _switchHotkey.HotkeyPressed += SwitchHotkey_HotkeyPressed;
            // init controls
            CtlUserInput.FocusText();
            CtlLauncherList.DataContext = _queryController.Results;
            CtlLauncherList.SelectedIndex = 0;
            
            var cultureInfo = Thread.CurrentThread.CurrentCulture;
            
        }

        private void ShowAndActivate()
        {
            if (Visibility == Visibility.Visible && !IsActive)
            {
                Activate();
                return;
            }
            if (!_activating && Visibility != Visibility.Visible)
            {
                _activating = true;
                Show();
                Activate();
                CtlUserInput.FocusText();
                _activating = false;
            }
        }

        private void ClearAndHide()
        {
            if (!_deactivating)
            {
                _deactivating = true;
                CtlUserInput.SetTextWithoutNotification("");
                // force clear
                _queryController.ClearCurrentQuery();
                // ensure render updates
                // otherwise we may see phantoms upon shown
                Dispatcher.InvokeAsync(() =>
                {
                    Hide();
                    _deactivating = false;
                }, DispatcherPriority.ContextIdle);
            }
        }

        private void SwitchHotkey_HotkeyPressed(object sender, EventArgs eventArgs)
        {
            ShowAndActivate();
        }

        private void CtlUserInput_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            
            
        }


        private void MainWindow_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // always focus on text input
            if (!CtlUserInput.IsTextFocused)
            {
                CtlUserInput.FocusText();
            }



            if (e.Key.Equals(Key.Down))
            {
                CtlLauncherList.SelectNext();
            }
            else if (e.Key.Equals(Key.Up))
            {
                CtlLauncherList.SelectPrevious();
            }
            else if (e.Key.Equals(Key.Escape))
            {
                ClearAndHide();
            }
        }

        private void MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                LauncherData launcherData = CtlLauncherList.SelectedLauncher;
                if (launcherData != null)
                {
                    var executedEvent = new LauncherExecutedEvent(launcherData, _queryController.CurrentQuery);
                    LauncherZApp.Instance.PluginManager.DistributeEventTo(launcherData.PluginId, executedEvent);
                    if (executedEvent.IsDefaultPrevented)
                    {
                        if (executedEvent.HideWindow)
                        {
                            ClearAndHide();
                        }
                        if (executedEvent.ModifiedInput != CtlUserInput.Text)
                        {
                            CtlUserInput.SetTextAndNotify(executedEvent.ModifiedInput);
                        }
                    }
                    else
                    {
                        ClearAndHide();
                    }
                    e.Handled = true;
                }
            }
        }

        private void CtlUserInput_OnDelayedTextChanged(object sender, RoutedEventArgs e)
        {
            if (Visibility != Visibility.Visible)
                return;
            
            if (string.IsNullOrWhiteSpace(CtlUserInput.Text))
            {
                _queryController.ClearCurrentQuery();
            }
            else
            {
                _queryController.DistributeQuery(LauncherQuery.Create(CtlUserInput.Text));
            }
        }

        private void CtlLauncherList_SelectionChanged(object sender, LauncherSelectionChangedEventArgs e)
        {
            if (e.OldItem != null)
            {
                LauncherZApp.Instance.PluginManager.DistributeEventTo(
                    e.OldItem.PluginId, new LauncherDeselectedEvent(e.OldItem));
            }
            if (e.NewItem != null)
            {
                LauncherZApp.Instance.PluginManager.DistributeEventTo(
                    e.NewItem.PluginId, new LauncherSelectedEvent(e.NewItem));
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _switchHotkey.Unregister();
        }

        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
#if DEBUG
#else
            ClearAndHide();
#endif
        }
    }

    class ActionCommand : ICommand
    {
        private Action<object> _action;

        public ActionCommand(Action<object> action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action(parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}
