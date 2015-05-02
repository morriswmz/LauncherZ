using System;
using System.Collections.Generic;
using System.Diagnostics;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Plugin.Template;
using LauncherZLib.Utils;

namespace CorePlugins.CoreCommands.Commands
{
    public sealed class CpuCommandHandler : CoreCommandHandler, IDisposable
    {
        private readonly PerformanceCounter _cpuCounter;
        private bool _disposed = false;

        public CpuCommandHandler(IExtendedServiceProvider serviceProvider) : base(serviceProvider)
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            
            if (disposing)
            {
                _cpuCounter.Dispose();
            }

            _disposed = true;
        }

        public override IEnumerable<CommandLauncherData> HandleQuery(LauncherQuery query)
        {
            return new []
            {
                new CommandLauncherData(query.Arguments, 1.0)
                {
                    Title = Localization["CpuCommandTitle"],
                    Description = Localization["CpuCommandDescription"],
                    DescriptionFont = "Segoe UI Mono",
                    IconLocation = @"LauncherZ://IconGear",
                    Tickable = true,
                    CurrentTickRate = TickRate.Normal
                }
            };
        }

        public override PostLaunchAction HandleLaunch(CommandLauncherData cmdData)
        {
            Process.Start("taskmgr");
            return PostLaunchAction.Default;
        }

        public override void HandleTick(CommandLauncherData cmdData)
        {
            var cpu = (int)_cpuCounter.NextValue();
            var bar = StringUtils.CreateProgressBar("[= ]", 20, cpu / 100.0);
            cmdData.Description = string.Format("[{0}] {1}%\n{2}", bar, cpu, Localization["CpuCommandDescription"]);
        }

    }
}
