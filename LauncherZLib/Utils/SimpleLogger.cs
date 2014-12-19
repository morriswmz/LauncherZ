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
    /// </summary>
    public class SimpleLogger
    {

        private bool _isRunning = true;
        private bool _logFileValid = true;
        private string _logFile;
        private BlockingCollection<string> _messages = new BlockingCollection<string>();
        private string _messageToWrite;
        private CancellationTokenSource _csource = new CancellationTokenSource();
        private Task _logTask;

        public SimpleLogger(string logFile)
        {
            _logFile = logFile;
            _logFileValid = VerifyFilePath();
            if (_logFileValid)
                _logTask = Task.Factory.StartNew(LoggerTask, _csource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Log(string msg)
        {
            
            if (_logFileValid && _isRunning)
            {
                _messages.Add(string.Format("[{0}]{1}", DateTime.Now.ToString("s"), msg));
            }
            
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
            Log("[WARNING]" + msg);
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
                _logTask.Wait();
            }
            catch (Exception ex) { }
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
                        while (_messages.TryTake(out _messageToWrite, 10, _csource.Token))
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




    }
}
