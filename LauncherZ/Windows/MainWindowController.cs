

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using LauncherZLib;
using LauncherZLib.Event.Launcher;
using LauncherZLib.Launcher;
using LauncherZLib.Utils;

namespace LauncherZ.Windows
{
    /// <summary>
    /// Handles interaction logic of MainWindow.
    /// </summary>
    internal sealed class MainWindowController
    {

        private static readonly TimeSpan MinInputResponseDelay = new TimeSpan(0, 0, 0, 0, 100);

        private readonly LauncherZApp _app;
        private readonly MainWindow _mw;
        private readonly MainWindowModel _mwModel;

        private QueryController _queryController;
        private DispatcherTimer _inputDelayTimer;
        private DispatcherTimer _tickTimer;
        private GlobalHotkey _switchHotkey;
        private int _tickTimerDivider = 0;
        private TimeSpan _inputResponseDelay = new TimeSpan(0, 0, 0, 0, 200);
        private LauncherList _launchers;
        private LauncherData _lastSelectedLauncher;
        private bool _mwDeactivating;
        private bool _mwActivating;

        public MainWindowController(MainWindow mw, MainWindowModel mwModel, LauncherZApp app)
        {
            if (mw == null)
                throw new ArgumentNullException("mw");
            if (mwModel == null)
                throw new ArgumentNullException("mwModel");
            if (app == null)
                throw new ArgumentNullException("app");

            _mw = mw;
            _mwModel = mwModel;
            _app = app;

            Initialize();
        }
        
        public TimeSpan InputResponseDelay
        {
            get { return _inputResponseDelay; }
            set { _inputResponseDelay = value < MinInputResponseDelay ? MinInputResponseDelay : value; }
        }

        public void Initialize()
        {
            // link to model
            _lastSelectedLauncher = _mwModel.SelectedLauncher;
            _mwModel.Launchers = new LauncherList(new LauncherDataComparer(_app.PluginManager));
            _mwModel.Launchers.CollectionChanged += Launchers_CollectionChanged;
            _mwModel.PropertyChanged += Model_PropertyChanged;

            // set up query controller
            _queryController = new QueryController(_app.PluginManager, 100);
            _queryController.ResultUpdate += QueryController_ResultUpdate;

            // setup timers
            _inputDelayTimer = new DispatcherTimer();
            _inputDelayTimer.Tick += InputDelayTimer_Tick;
            _tickTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 50) };
            _tickTimer.Tick += TickTimer_Tick;
            
            LauncherZConfig config = _app.Configuration;
            // register global hotkey for MainWindow
            if (string.IsNullOrEmpty(config.ActivationKeyCombo))
            {
                config.ActivationKeyCombo = LauncherZConfig.DefaultActivationKeyCombo;
            }
            try
            {
                _switchHotkey = new GlobalHotkey(config.ActivationKeyCombo);
            }
            catch (Exception)
            {
                _app.Logger.Warning(
                    string.Format("{0} is not a vali key combination. Restoring activation hot key to default: {1}.",
                        config.ActivationKeyCombo, LauncherZConfig.DefaultActivationKeyCombo));
                config.ActivationKeyCombo = LauncherZConfig.DefaultActivationKeyCombo;
                _switchHotkey = new GlobalHotkey(config.ActivationKeyCombo);
            }
            _switchHotkey.Register(_mw, 0);
            _switchHotkey.HotkeyPressed += SwitchHotkey_HotkeyPressed;

            // hook window events
            _mw.PreviewKeyUp += MainWindow_PreviewKeyUp;
        }

        private void ClearAndHideMainWindow()
        {
            if (!_mwDeactivating)
            {
                _mwDeactivating = true;
                _mwModel.InputText = "";
                // force clear
                _queryController.ClearCurrentQuery();
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

        private void MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                LauncherData launcherData = _mwModel.SelectedLauncher;
                if (launcherData != null)
                {
                    PostLaunchAction action = _app.PluginManager
                        .GetPluginContainer(launcherData.PluginId)
                        .PluginInstance
                        .Launch(launcherData);
                    if (action.HideWindow)
                    {
                        ClearAndHideMainWindow();
                    }
                    else
                    {
                        
                    }
                    e.Handled = true;
                }
            }
        }

        


        private void QueryController_ResultUpdate(object sender, ResultUpdatedEventArgs e)
        {
            _mwModel.Launchers.AddRange(e.Updates);
            foreach (var launcherData in e.Updates)
            {
                _app.PluginManager.DistributeEventTo(launcherData.PluginId, new LauncherAddedEvent(launcherData));
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
                        _app.PluginManager.DistributeEventTo(_lastSelectedLauncher.PluginId,
                            new LauncherDeselectedEvent(_lastSelectedLauncher));
                    }
                    LauncherData newLauncher = _mwModel.SelectedLauncher;
                    if (newLauncher != null)
                    {
                        _app.PluginManager.DistributeEventTo(newLauncher.PluginId,
                            new LauncherSelectedEvent(newLauncher));
                    }
                    _lastSelectedLauncher = newLauncher;
                    break;
            }
        }

        private void Launchers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // results updated. check if we need to start/stop tick timer
            if (_launchers.Count > 0 && !_tickTimer.IsEnabled)
            {
                _tickTimer.Start();
            }
        }

        private void TickTimer_Tick(object sender, EventArgs eventArgs)
        {
            if (_launchers.Count == 0)
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
            foreach (var cmd in _launchers.Where(cmd => cmd.ExtendedProperties.Tickable))
            {
                bool shouldTick = cmd.ExtendedProperties.CurrentTickRate == TickRate.Fast;
                shouldTick = shouldTick || (tickNormal && cmd.ExtendedProperties.CurrentTickRate == TickRate.Normal);
                shouldTick = shouldTick || (tickSlow && cmd.ExtendedProperties.CurrentTickRate == TickRate.Slow);
                if (shouldTick)
                {
                    _app.PluginManager.DistributeEventTo(cmd.PluginId, new LauncherTickEvent(cmd));
                }
            }
        }

        private void InputDelayTimer_Tick(object sender, EventArgs eventArgs)
        {
            if (string.IsNullOrWhiteSpace(_mwModel.InputText))
            {
                _queryController.ClearCurrentQuery();
                _mwModel.Launchers.Clear();
            }
            else
            {
                _queryController.DistributeQuery(new LauncherQuery(_mwModel.InputText.Trim()));
            }
            _inputDelayTimer.Stop();
        }

        private void SwitchHotkey_HotkeyPressed(object sender, EventArgs e)
        {
            
        }


    }

}
