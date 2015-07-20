using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using LauncherZ.Configuration;
using LauncherZLib.Event;
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
        private const string LauncherZString = "LauncherZ";

        private MainWindow _mw;
        private MainWindowModel _mwModel;
        private LauncherZConfig _config;
        private LaunchHistoryManager _historyManager;
        private PluginManager _pluginManager;
        private IEventBus _globalEventBus;
        private QueryDistributor _queryDistributor;
        private ResultManager _resultManager;
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
        private bool _mwLaunching;

        public MainWindowController(LauncherZApp app)
        {
            if (app == null)
                throw new ArgumentNullException("app");
            
            _config = app.Configuration;
            _historyManager = app.LaunchHistoryManager;
            _pluginManager = app.PluginManager;
            _logger = app.Logger;
            _globalEventBus = app.GlobalEventBus;
        }

        public void Attach(MainWindow mw)
        {
            if (_attached)
                Detach();

            // prepare components
            _queryDistributor = new QueryDistributor(_pluginManager, _logger, _config.MaxResultCount);
            _resultManager = new ResultManager(_pluginManager, _queryDistributor);
            _globalEventBus.Register(_queryDistributor);

            // set up data context
            _mw = mw;
            if (_mw.DataContext == null)
            {
                _mw.DataContext = new MainWindowModel();
            }
            _mwModel = (MainWindowModel) _mw.DataContext;

            // link to model
            _lastSelectedLauncher = _mwModel.SelectedLauncher;
            _mwModel.Launchers = _resultManager.Results;
            ((INotifyCollectionChanged) _mwModel.Launchers).CollectionChanged += Launchers_CollectionChanged;
            _mwModel.PropertyChanged += Model_PropertyChanged;

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

            _globalEventBus.Unregister(_queryDistributor);

            ((INotifyCollectionChanged)_mwModel.Launchers).CollectionChanged -= Launchers_CollectionChanged;
            _mwModel.PropertyChanged -= Model_PropertyChanged;
            _mwModel.Launchers = null;
            _mwModel = null;

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

            _attached = false;
        }
        
        /// <summary>
        /// Gets or sets the response delay after input updating.
        /// </summary>
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
                ClearAndReset();
                // ensure render updates
                // otherwise we may see phantoms upon shown
                _mw.Dispatcher.InvokeAsync(() =>
                {
                    if (_mw != null)
                    {
                        _mw.Hide();
                    }
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

        private void ClearAndReset()
        {
            _queryDistributor.ClearCurrentQuery();
            _mwModel.HintText = LauncherZString;
            _mwModel.IsInputEnabled = true;
        }

        private void DistributeNewQuery()
        {
            LauncherQuery lastQuery = _queryDistributor.CurrentQuery;
            _queryDistributor.ClearCurrentQuery();
            if (!string.IsNullOrWhiteSpace(_mwModel.InputText))
            {
                var newQuery = lastQuery == null
                    ? new LauncherQuery(_mwModel.InputText.Trim())
                    : new LauncherQuery(lastQuery, _mwModel.InputText.Trim());
                _queryDistributor.DistributeQuery(newQuery);
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
                _mwLaunching = true;
                LauncherData launcherData = _mwModel.SelectedLauncher;
                if (launcherData == null)
                {
                    _mwLaunching = false;
                    return;
                }
                var pluginId = _resultManager.GetPluginIdOf(launcherData);
                PostLaunchAction action = _pluginManager
                    .GetPluginContainer(pluginId)
                    .PluginInstance
                    .Launch(launcherData, new LaunchContext(_queryDistributor.CurrentQuery));
                _historyManager.PushHistory(_mwModel.InputText, pluginId);
                if (action.ActionType.HasFlag(PostLaunchActionType.HideWindow))
                {
                    ClearAndHideMainWindow();
                }
                else
                {
                    _mwModel.IsInputEnabled = !action.ActionType.HasFlag(PostLaunchActionType.LockInput);
                    if (action.ActionType.HasFlag(PostLaunchActionType.ModifyHint))
                    {
                        _mwModel.HintText = string.IsNullOrEmpty(action.ModifiedHint)
                            ? LauncherZString
                            : action.ModifiedHint;
                    }
                    if (action.ActionType.HasFlag(PostLaunchActionType.ModifyUri))
                    {
                        // modify query uri
                        try
                        {
                            var newQuery = new LauncherQuery(new Uri(action.ModifiedUri));
                            if (!string.IsNullOrEmpty(newQuery.OriginalInput))
                            {
                                _mwModel.InputText = newQuery.OriginalInput;
                            }
                            _queryDistributor.DistributeQuery(newQuery);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error("Invalid query uri \"{0}\". Details:{1}{2}",
                                action.ModifiedUri, Environment.NewLine, ex);
                            _queryDistributor.ClearCurrentQuery();
                        }
                    }
                    else if (action.ActionType.HasFlag(PostLaunchActionType.ModifyInput))
                    {
                        // modify input
                        if (string.IsNullOrWhiteSpace(action.ModifiedInput))
                        {
                            _queryDistributor.ClearCurrentQuery();
                        }
                        else
                        {
                            var newQuery = _queryDistributor.CurrentQuery == null
                                ? new LauncherQuery(action.ModifiedInput)
                                : new LauncherQuery(_queryDistributor.CurrentQuery, action.ModifiedInput);
                            _queryDistributor.DistributeQuery(newQuery);
                        }
                    }
                }
                _mwLaunching = false;
                e.Handled = true;
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

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "InputText":
                    if (_mwDeactivating || _mwLaunching)
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
                        _pluginManager.DistributeEventTo(_resultManager.GetPluginIdOf(_lastSelectedLauncher),
                            new LauncherDeselectedEvent(_lastSelectedLauncher));
                    }
                    LauncherData newLauncher = _mwModel.SelectedLauncher;
                    if (newLauncher != null)
                    {
                        _pluginManager.DistributeEventTo(_resultManager.GetPluginIdOf(newLauncher),
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
            foreach (
                var pair in
                    _resultManager.Results.Where(l => l.Tickable)
                        .Select(l => new KeyValuePair<string, LauncherData>(_resultManager.GetPluginIdOf(l), l)))
            {
                bool shouldTick = pair.Value.CurrentTickRate == TickRate.Fast;
                shouldTick = shouldTick || (tickNormal && pair.Value.CurrentTickRate == TickRate.Normal);
                shouldTick = shouldTick || (tickSlow && pair.Value.CurrentTickRate == TickRate.Slow);
                if (shouldTick)
                {
                    _pluginManager.DistributeEventTo(pair.Key, new LauncherTickEvent(pair.Value));
                }
            }
        }

        private void InputDelayTimer_Tick(object sender, EventArgs eventArgs)
        {
            DistributeNewQuery();
            _inputDelayTimer.Stop();
        }

        private void SwitchHotkey_HotkeyPressed(object sender, EventArgs e)
        {
            ShowAndActivateMainWindow();
        }

        #endregion

    }

}
