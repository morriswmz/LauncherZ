﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using LauncherZ.Icon;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin.Modules;
using LauncherZLib.Plugin.Service;
using LauncherZLib.Utils;

namespace CorePlugins.CoreCommands.Commands
{
    public sealed class CpuCommandHandler : CoreCommandHandler, IDisposable
    {
        private PerformanceCounter _cpuCounter;
        private bool _disposed = false;

        public CpuCommandHandler(IExtendedServiceProvider serviceProvider) : base(serviceProvider)
        {
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
                _cpuCounter = null;
            }

            _disposed = true;
        }

        public override IEnumerable<LauncherData> HandleQuery(LauncherQuery query)
        {
            return new []
            {
                new LauncherData(1.0)
                {
                    Title = Localization["CpuCommandTitle"],
                    Description = Localization["CpuCommandDescription"],
                    IconLocation = LauncherZIconSet.Gear.ToString(),
                    Tickable = true,
                    CurrentTickRate = TickRate.Normal
                }
            };
        }

        public override PostLaunchAction HandleLaunch(LauncherData data, LaunchContext context)
        {
            Process.Start("taskmgr");
            return PostLaunchAction.Default;
        }

        public override void HandleTick(LauncherData data)
        {
            // only initialize when needed
            if (_cpuCounter == null)
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            }
            var cpu = (int)_cpuCounter.NextValue();
            var bar = StringUtils.CreateProgressBar("[= ]", 20, cpu / 100.0);
            data.Description = string.Format("[{0}] {1}%\n{2}", bar, cpu, Localization["CpuCommandDescription"]);
        }

    }
}
