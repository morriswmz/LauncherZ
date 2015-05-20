

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using LauncherZ.Configuration;
using LauncherZLib;
using LauncherZLib.Event.Launcher;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin;
using LauncherZLib.Utils;

namespace LauncherZ.Windows
{
    /// <summary>
    /// Handles interaction logic of MainWindow.
    /// </summary>
    internal sealed class MainWindowController
    {

        private static readonly TimeSpan MinInputResponseDelay = new TimeSpan(0, 0, 0, 0, 100);

        private MainWindow _mw;
        private MainWindowModel _mwModel;
        private LauncherZConfig _config;
        private LaunchHistoryManager _historyManager;
        private PluginManager _pluginManager;
        private QueryDistributor _queryDistributor;
        private ILogger _logger;
        private DispatcherTimer _inputDelayTimer;
        private DispatcherTimer _tickTimer;
        private GlobalHotkey _switchHotkey;
        private bool _attached = false;
        private int _tickTimerDivider = 0;
        private TimeSpan _inputResponseDelay = new TimeSpan(0, 0, 0, 0, 200);
        private LauncherData _lastSelectedLauncher;
        private bool _mwDeactivating;
        private bool _mwActivating;

        public MainWindowController(LauncherZConfig config, QueryDistributor queryDistributor,
            LaunchHistoryManager historyManager, PluginManager pluginManager, ILogger logger)
        {
            if (config == null)
                throw new ArgumentNullException("config");
            if (queryDistributor == null)
                throw new ArgumentNullException("queryDistributor");
            if (historyManager == null)
                throw new ArgumentNullException("historyManager");
            if (pluginManager == null)
                throw new ArgumentNullException("pluginManager");
            if (logger == null)
                throw new ArgumentNullException("logger");

            _config = config;
            _queryDistributor = queryDistributor;
            _historyManager = historyManager;
            _pluginManager = pluginManager;
            _logger = logger;
        }

        public void Attach(MainWindow mw)
        {
            if (_attached)
                Detach();

            // set up data context
            _mw = mw;
            if (_mw.DataContext == null)
            {
                _mw.DataContext = new MainWindowModel();
            }
            _mwModel = (MainWindowModel) _mw.DataContext;

            // link to model
            _lastSelectedLauncher = _mwModel.SelectedLauncher;
            _mwModel.Launchers = new LauncherList(new LauncherDataComparer(_pluginManager));
            _mwModel.Launchers.CollectionChanged += Launchers_CollectionChanged;
            _mwModel.PropertyChanged += Model_PropertyChanged;

            // set up query controller
            _queryDistributor = new QueryDistributor(_pluginManager, 100);
            _queryDistributor.ResultUpdate += QueryDistributorResultUpdate;

            // setup timers
            _inputDelayTimer = new DispatcherTimer();
            _inputDelayTimer.Tick += InputDelayTimer_Tick;
            _tickTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 50) };
            _tickTimer.Tick += TickTimer_Tick;

            // register global hotkey for MainWindow
            if (string.IsNullOrEmpty(_config.ActivationKeyCombo))
            {
                _config.ActivationKeyCombo = LauncherZConfig.DefaultActivationKeyCombo;
            }
            try
            {
                _switchHotkey = new GlobalHotkey(_config.ActivationKeyCombo);
            }
            catch (Exception)
            {
                _logger.Warning(
                    "{0} is not a vali key combination. Restoring activation hot key to default: {1}.",
                    _config.ActivationKeyCombo, LauncherZConfig.DefaultActivationKeyCombo);
                _config.ActivationKeyCombo = LauncherZConfig.DefaultActivationKeyCombo;
                _switchHotkey = new GlobalHotkey(_config.ActivationKeyCombo);
            }
            _switchHotkey.Register(_mw, 0);
            _switchHotkey.HotkeyPressed += SwitchHotkey_HotkeyPressed;

            // hook window events
            _mw.PreviewKeyUp += MainWindow_PreviewKeyUp;
            _mw.PreviewKeyDown += MainWindow_PreviewKeyDown;
            _mw.Deactivated += MainWindow_Deactivated;
            _mw.Closed += MainWindow_Closed;

            _attached = true;
        }

        public void Detach()
        {
            if (!_attached)
                return;

            if (_switchHotkey.IsRegistered)
            {
                _switchHotkey.Unregister();
                _switchHotkey.Dispose();
            }

            _mwModel.Launchers.CollectionChanged -= Launchers_CollectionChanged;
            _mwModel.PropertyChanged -= Model_PropertyChanged;
            _queryDistributor.ResultUpdate -= QueryDistributorResultUpdate;
            _inputDelayTimer.Tick -= InputDelayTimer_Tick;
            _inputDelayTimer.Stop();
            _inputDelayTimer = null;
            _tickTimer.Tick -= TickTimer_Tick;
            _tickTimer.Stop();
            _tickTimer = null;

            _mw.PreviewKeyUp -= MainWindow_PreviewKeyUp;
            _mw.PreviewKeyDown -= MainWindow_PreviewKeyDown;
            _mw.Deactivated -= MainWindow_Deactivated;
            _mw.Closed -= MainWindow_Closed;

            _mw = null;
            _mwModel = null;

            _attached = false;
        }
     
        public TimeSpan InputResponseDelay
        {
            get { return _inputResponseDelay; }
            set { _inputResponseDelay = value < MinInputResponseDelay ? MinInputResponseDelay : value; }
        }

        private void ClearAndHideMainWindow()
        {
            if (!_mwDeactivating)
            {
                _mwDeactivating = true;
                _mwModel.InputText = "";
                // force clear
                _queryDistributor.ClearCurrentQuery();
                _mwModel.Launchers.Clear();
                // ensure render updates
                // otherwise we may see phantoms upon shown
                _mw.Dispatcher.InvokeAsync(() =>
                {
                    _mw.Hide();
                    _mwDeactivating = false;
                }, DispatcherPriority.ContextIdle);
            }
        }

        private void ShowAndActivateMainWindow()
        {
            if (_mw.Visibility == Visibility.Visible && !_mw.IsActive)
            {
                _mw.Activate();
                return;
            }
            if (!_mwActivating && _mw.Visibility != Visibility.Visible)
            {
                _mwActivating = true;
                _mw.Show();
                _mw.Activate();
                _mwActivating = false;
            }
        }

        private void SelectNextLauncher()
        {
            if (_mwModel.Launchers != null && _mwModel.Launchers.Count > 0)
            {
                int idx = _mwModel.Launchers.IndexOf(_mwModel.SelectedLauncher);
                _mwModel.SelectedLauncher = _mwModel.Launchers[idx < _mwModel.Launchers.Count - 1 ? idx + 1 : 0];
            }
        }

        private void SelectPreviousLauncher()
        {
            if (_mwModel.Launchers != null && _mwModel.Launchers.Count > 0)
            {
                int idx = _mwModel.Launchers.IndexOf(_mwModel.SelectedLauncher);
                _mwModel.SelectedLauncher = _mwModel.Launchers[idx > 0 ? idx - 1 : _mwModel.Launchers.Count - 1];
            }
        }

        #region Event Handlers

        private void MainWindow_Deactivated(object sender, EventArgs eventArgs)
        {
#if DEBUG
#else
            ClearAndHideMainWindow();
#endif

        }

        private void MainWindow_Closed(object sender, EventArgs eventArgs)
        {
            // save launch history when closed
            Detach();
        }

        private void MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                LauncherData launcherData = _mwModel.SelectedLauncher;
                if (launcherData != null)
                {
                    PostLaunchAction action = _pluginManager
                        .GetPluginContainer(launcherData.PluginId)
                        .PluginInstance
                        .Launch(launcherData);
                    _historyManager.PushHistory(_mwModel.InputText, launcherData.PluginId);
                    if (action.HideWindow)
                    {
                        ClearAndHideMainWindow();
                    }
                    else
                    {
                        if (action.ModifyInput)
                        {
                            _mwModel.InputText = action.ModifiedInput;
                        }
                        if (action.EnterExclusiveMode)
                        {
                            
                        }
                    }
                    e.Handled = true;
                }
            }
        }
        
        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    HandleDownKeyPressed(e);
                    break;
                case Key.Up:
                    HandleUpKeyPressed(e);
                    break;
                case Key.Escape:
                    HandleEscapeKeyPressed(e);
                    break;
                default:
                    _mw.FocusInput();
                    break;
            }
        }

        private void HandleDownKeyPressed(KeyEventArgs e)
        {
            SelectNextLauncher();
            e.Handled = true;
        }

        private void HandleUpKeyPressed(KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(_mwModel.InputText) && _historyManager.History.Any())
            {
                _mwModel.InputText = _historyManager.History.First();
                _mw.SelectInputText();
            }
            else
            {
                SelectPreviousLauncher();
            }
            e.Handled = true;
        }

        private void HandleEscapeKeyPressed(KeyEventArgs e)
        {
            ClearAndHideMainWindow();
            e.Handled = true;
        }

        private void QueryDistributorResultUpdate(object sender, ResultUpdatedEventArgs e)
        {
            _mwModel.Launchers.AddRange(e.Updates);
            foreach (var launcherData in e.Updates)
            {
                _pluginManager.DistributeEventTo(launcherData.PluginId, new LauncherAddedEvent(launcherData));
            }
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "InputText":
                    if (_mwDeactivating)
                        return;
                    _inputDelayTimer.Interval = InputResponseDelay;
                    _inputDelayTimer.Start();
                    break;
                case "Launchers":
                    // this should never happen
                    throw new Exception("The launcher list instance assign to model should never be changed." +
                                        "Only its content can be altered.");
                case "SelectedLauncher":
                    // selection changed
                    if (_lastSelectedLauncher != null)
                    {
                        _pluginManager.DistributeEventTo(_lastSelectedLauncher.PluginId,
                            new LauncherDeselectedEvent(_lastSelectedLauncher));
                    }
                    LauncherData newLauncher = _mwModel.SelectedLauncher;
                    if (newLauncher != null)
                    {
                        _pluginManager.DistributeEventTo(newLauncher.PluginId,
                            new LauncherSelectedEvent(newLauncher));
                    }
                    _lastSelectedLauncher = newLauncher;
                    break;
            }
        }

        private void Launchers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // results updated. check if we need to start/stop tick timer
            if (_mwModel.Launchers.Count > 0 && !_tickTimer.IsEnabled)
            {
                _tickTimer.Start();
            }
        }

        private void TickTimer_Tick(object sender, EventArgs eventArgs)
        {
            if (_mwModel.Launchers.Count == 0)
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
            foreach (var launcherData in _mwModel.Launchers.Where(l => l.Tickable))
            {
                bool shouldTick = launcherData.CurrentTickRate == TickRate.Fast;
                shouldTick = shouldTick || (tickNormal && launcherData.CurrentTickRate == TickRate.Normal);
                shouldTick = shouldTick || (tickSlow && launcherData.CurrentTickRate == TickRate.Slow);
                if (shouldTick)
                {
                    _pluginManager.DistributeEventTo(launcherData.PluginId, new LauncherTickEvent(launcherData));
                }
            }
        }

        private void InputDelayTimer_Tick(object sender, EventArgs eventArgs)
        {
            _queryDistributor.ClearCurrentQuery();
            _mwModel.Launchers.Clear();
            if (!string.IsNullOrWhiteSpace(_mwModel.InputText))
            {
                _queryDistributor.DistributeQuery(new LauncherQuery(_mwModel.InputText.Trim()));
            }
            _inputDelayTimer.Stop();
        }

        private void SwitchHotkey_HotkeyPressed(object sender, EventArgs e)
        {
            ShowAndActivateMainWindow();
        }

        #endregion

    }

}
