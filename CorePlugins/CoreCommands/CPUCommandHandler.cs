﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LauncherZLib.API;
using LauncherZLib.Event;
using LauncherZLib.Launcher;

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
            };
        }

        public void HandleTick(LauncherTickEvent e, IPluginContext context)
        {
            var cpu = (int)_cpuCounter.NextValue();
            var bars = 20 * cpu / 100;
            var progress = new char[20];
            int i = 0;
            for (i = 0; i < bars; i++)
                progress[i] = '=';
            for (; i < 20; i++)
                progress[i] = ' ';
            e.LauncherData.Description = string.Format("{0}% \\[{1}\\]", cpu, new string(progress));
        }

        public void HandleExecute(LauncherExecutedEvent e, IPluginContext context)
        {
            // do nothing
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