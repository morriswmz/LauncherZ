using System;
using System.Collections.Generic;
using System.Diagnostics;
using LauncherZLib.Event.Launcher;
using LauncherZLib.Launcher;
using LauncherZLib.Utils;

namespace CorePlugins.CoreCommands
{
    public class CpuCommandHandler : CommandHandler, IDisposable
    {
        private readonly PerformanceCounter _cpuCounter;
        private bool _disposed = false;

        public CpuCommandHandler(ICommandPlugin plugin) : base(plugin)
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        }

        ~CpuCommandHandler()
        {
            Dispose(false);
        }

        public override string CommandName { get { return "CPU"; } }


        public override IEnumerable<LauncherData> HandleQuery(LauncherQuery query)
        {
            return new LauncherData[]
            {
                new LauncherData(
                    Plugin.Localization["CpuCommandTitle"],
                    Plugin.Localization["CpuCommandDescription"],
                    @"LauncherZ://IconGear", 1.0,
                    new CommandExtendedProperties(true, TickRate.Normal, query.Arguments)
                )
                {
                    DescriptionFont = "Segoe UI Mono"
                }
            };
        }

        public override void HandleTick(LauncherTickEvent e)
        {
            var cpu = (int)_cpuCounter.NextValue();
            var bar = StringUtils.CreateProgressBar("[= ]", 20, cpu/100.0);
            e.LauncherData.Description = string.Format("[{0}] {1}%\n{2}", bar, cpu, Plugin.Localization["CpuCommandDescription"]);
        }

        public override void HandleExecute(LauncherExecutedEvent e)
        {
            Process.Start("taskmgr");
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
