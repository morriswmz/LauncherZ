using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using LauncherZ.Controls;
using LauncherZLib;
using LauncherZLib.Event.Launcher;
using LauncherZLib.Launcher;
using LauncherZLib.Utils;

namespace LauncherZ.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private DetailWindow detailWindow;
        private readonly QueryController _queryController;
        private readonly GlobalHotkey _switchHotkey;
        private readonly DispatcherTimer _tickTimer;
        private int _tickTimerDivider = 0;
        private List<Key> _pressedKeys = new List<Key>();
        private bool _activating = false;
        private bool _deactivating = false;

        public MainWindow()
        {
            InitializeComponent();
            LauncherZConfig config = LauncherZApp.Instance.Configuration;
            // register global hotkey
            if (string.IsNullOrEmpty(config.ActivationKeyCombo))
            {
                // todo: log
                config.ActivationKeyCombo = LauncherZConfig.DefaultActivationKeyCombo;
            }
            try
            {
                _switchHotkey = new GlobalHotkey(config.ActivationKeyCombo);
            }
            catch (Exception ex)
            {
                // todo: log
                config.ActivationKeyCombo = LauncherZConfig.DefaultActivationKeyCombo;
                _switchHotkey = new GlobalHotkey(config.ActivationKeyCombo);
            }
            _switchHotkey.Register(this, 0);
            _switchHotkey.HotkeyPressed += SwitchHotkey_HotkeyPressed;
            // setup query controller
            _queryController = new QueryController(LauncherZApp.Instance.PluginManager, 20);
            _queryController.Results.CollectionChanged += Results_CollectionChanged;
            // init controls
            CtlUserInput.FocusText();
            CtlLauncherList.DataContext = _queryController.Results;
            CtlLauncherList.SelectedIndexOld = 0;
            // setup tick timer
            _tickTimer = new DispatcherTimer();
            _tickTimer.Start();
            _tickTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
            _tickTimer.Tick += TickTimer_Tick;
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

        #region Event Handlers

        private void TickTimer_Tick(object sender, EventArgs e)
        {
            if (_queryController.Results.Count == 0)
            {
                _tickTimer.Stop();
                _tickTimerDivider = 0;
                return;
            }
            _tickTimerDivider++;
            bool tickNormal = _tickTimerDivider % 5 == 0;
            bool tickSlow = false;
            if (_tickTimerDivider == 20)
            {
                _tickTimerDivider = 0;
                tickSlow = true;
            }
            foreach (var cmd in _queryController.Results.Where(cmd => cmd.ExtendedProperties.Tickable))
            {
                bool shouldTick = cmd.ExtendedProperties.CurrentTickRate == TickRate.Fast;
                shouldTick = shouldTick || (tickNormal && cmd.ExtendedProperties.CurrentTickRate == TickRate.Normal);
                shouldTick = shouldTick || (tickSlow && cmd.ExtendedProperties.CurrentTickRate == TickRate.Slow);
                if (shouldTick)
                {
                    LauncherZApp.Instance.PluginManager.DistributeEventTo(cmd.PluginId, new LauncherTickEvent(cmd));
                }
            }
        }

        private void Results_CollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            // results updated. check if we need to start/stop tick timer
            if (_queryController.Results.Count > 0 && !_tickTimer.IsEnabled)
            {
                _tickTimer.Start();
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
                LauncherData launcherData = CtlLauncherList.SelectedLauncherOld;
                if (launcherData != null)
                {
                    PostLaunchAction action = LauncherZApp.Instance.PluginManager
                        .GetPluginContainer(launcherData.PluginId)
                        .PluginInstance
                        .Launch(launcherData);
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
                _queryController.DistributeQuery(new LauncherQuery(CtlUserInput.Text));
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

        private void MainWindow_Deactivated(object sender, EventArgs e)
        {
#if DEBUG
#else
            ClearAndHide();
#endif
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _switchHotkey.Unregister().Dispose();
        }

        #endregion
    }

}
