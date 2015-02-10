using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using LauncherZLib.API;
using LauncherZLib.Event.Launcher;
using LauncherZLib.Launcher;
using LauncherZLib.Utils;

namespace CorePlugins.CoreCommands
{
    public class CpuCommandHandler : ICommandHandler, IDisposable
    {
        private readonly PerformanceCounter _cpuCounter;
        private bool _disposed = false;

        public CpuCommandHandler()
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        }

        ~CpuCommandHandler()
        {
            Dispose(false);
        }

        public string CommandName { get { return "CPU"; } }


        public IEnumerable<LauncherData> HandleQuery(LauncherQuery query, IPluginContext context)
        {
            return new LauncherData[]
            {
                new LauncherData(context.Localization["CpuCommandTitle"], "", @"LauncherZ|IconGear", 1.0,
                        new CommandExtendedProperties(true, TickRate.Normal, query.Arguments))
                {
                    DescriptionFont = "Helvatica"
                }
            };
        }

        public void HandleTick(LauncherTickEvent e, IPluginContext context)
        {
            var cpu = (int)_cpuCounter.NextValue();
            var bar = StringUtils.CreateProgressBar("[=-]", 20, cpu/100.0);
            e.LauncherData.Description = string.Format("[{1}] {0}%", cpu, bar);
        }

        public void HandleExecute(LauncherExecutedEvent e, IPluginContext context)
        {
            e.PreventDefault();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            
            if (disposing)
            {
                _cpuCounter.Dispose();
            }

            _disposed = true;
        }

    }
}
