using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Converters;

namespace LauncherZLib.Utils
{
    /// <summary>
    /// Provides simple logging functionality.
    /// Also implements ILoggerProvider to create wrapped SimpleLogger
    /// for specific category.
    /// </summary>
    public class SimpleLogger : ILogger, ILoggerProvider
    {

        private bool _isRunning = true;
        private readonly bool _logFileValid = true;
        private readonly string _logFile;
        private readonly BlockingCollection<string> _messages = new BlockingCollection<string>();
        private string _messageToWrite;
        private readonly CancellationTokenSource _csource = new CancellationTokenSource();
        private Task _logTask;

        private readonly Dictionary<string, ILogger> _subLoggers = new Dictionary<string, ILogger>(); 


        public SimpleLogger(string logFile)
        {
            _logFile = logFile;
            _logFileValid = VerifyFilePath();
            if (_logFileValid)
            {
                _logTask = Task.Factory.StartNew(LoggerTask, _csource.Token, TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
            }
            else
            {
                _isRunning = false;
            }
        }

        public bool IsRunning
        {
            get { return _isRunning; }
        }

        public ILogger CreateLogger(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category cannot be null or white space.");

            if (_subLoggers.ContainsKey(category))
            {
                return _subLoggers[category];
            }
            else
            {
                var sl = new WrappedSimpleLogger(this, category);
                _subLoggers.Add(category, sl);
                return sl;
            }
        }

        public void Log(string msg)
        {
            if (_logFileValid && _isRunning)
            {
                _messages.Add(string.Format("[{0}]{1}", DateTime.Now.ToString("s"), msg));
            }
        }

        public void Fine(string msg)
        {
            Log("[FINE]" + msg);
        }

        public void Info(string msg)
        {
            Log("[INFO]" + msg);
        }

        public void Warning(string msg)
        {
            Log("[WARNING]" + msg);
        }

        public void Error(string msg)
        {
            Log("[ERROR]" + msg);
        }

        public void Severe(string msg)
        {
            Log("[SEVERE]" + msg);
        }

        public void Close()
        {
            try
            {
                _csource.Cancel(false);
                _messages.CompleteAdding();
                _isRunning = false;
                // wait until all messages are written
                _logTask.Wait();
            }
            catch (Exception ex)
            {
                // we are closing. safely ignore exceptions
            }
            finally
            {
                _isRunning = false;
            }
        }

        private bool VerifyFilePath()
        {
            if (File.Exists(_logFile))
                return true;
            try
            {
                File.Create(_logFile).Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void LoggerTask()
        {
            while (true)
            {
                try
                {
                    // wait for new messages
                    // or continue with last message
                    if (_messageToWrite == null)
                        _messageToWrite = _messages.Take(_csource.Token);
                    // open stream
                    using (var sw = new StreamWriter(_logFile, true))
                    {
                        sw.WriteLine(_messageToWrite);
                        _messageToWrite = null;
                        // fetch remaining messages
                        while (_messages.TryTake(out _messageToWrite, 50))
                        {
                            sw.WriteLine(_messageToWrite);
                            _messageToWrite = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (!(ex is OperationCanceledException || ex is InvalidOperationException))
                    {
                        throw;
                    }
                    else
                    {
                        _isRunning = false;
                        break;
                    }
                }
            }
        }

        public class WrappedSimpleLogger : ILogger
        {
            private readonly ILogger _logger;
            private readonly string _category;

            public WrappedSimpleLogger(ILogger logger, string category)
            {
                _logger = logger;
                _category = category;
            }

            public bool IsRunning { get { return _logger.IsRunning; } }

            public void Log(string msg)
            {
                _logger.Log(string.Format("[{0}]{1}", _category, msg));
            }

            public void Info(string msg)
            {
                _logger.Log(string.Format("{0}[{1}]{2}", "[INFO]", _category, msg));
            }

            public void Fine(string msg)
            {
                _logger.Log(string.Format("{0}[{1}]{2}", "[FINE]", _category, msg));
            }

            public void Warning(string msg)
            {
                _logger.Log(string.Format("{0}[{1}]{2}", "[WARNING]", _category, msg));
            }

            public void Error(string msg)
            {
                _logger.Log(string.Format("{0}[{1}]{2}", "[ERROR]", _category, msg));
            }

            public void Severe(string msg)
            {
                _logger.Log(string.Format("{0}[{1}]{2}", "[SEVERE]", _category, msg));
            }
        }


    }
}
