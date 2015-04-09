using System;
using System.Collections.Generic;
using System.Diagnostics;
using LauncherZLib.Event;
using LauncherZLib.Event.Launcher;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Plugin.Template;
using LauncherZLib.Utils;

namespace CorePlugins.CoreCommands
{
    public class CpuCommandHandler : CoreCommandHandler, IDisposable
    {
        private readonly PerformanceCounter _cpuCounter;
        private bool _disposed = false;

        public CpuCommandHandler(IPluginServiceProvider serviceProvider) : base(serviceProvider)
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        }

        ~CpuCommandHandler()
        {
            Dispose(false);
        }

        public override string CommandName
        {
            get { return "CPU"; }
        }

        public override bool SubscribeToEvents
        {
            get { return true; }
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

        public override IEnumerable<LauncherData> HandleQuery(LauncherQuery query)
        {
            return new LauncherData[]
            {
                new LauncherData(
                    Localization["CpuCommandTitle"],
                    Localization["CpuCommandDescription"],
                    @"LauncherZ://IconGear", 1.0,
                    new CommandExtendedProperties(query.Arguments, true, TickRate.Normal)
                )
                {
                    DescriptionFont = "Segoe UI Mono"
                }
            };
        }

        public override PostLaunchAction HandleLaunch(LauncherData launcherData)
        {
            Process.Start("taskmgr");
            return PostLaunchAction.Default;
        }

        [SubscribeEvent]
        public void LauncherTickEventHandler(LauncherTickEvent e)
        {
            var cpu = (int)_cpuCounter.NextValue();
            var bar = StringUtils.CreateProgressBar("[= ]", 20, cpu / 100.0);
            e.LauncherData.Description = string.Format("[{0}] {1}%\n{2}", bar, cpu, Localization["CpuCommandDescription"]);
        }

    }
}
