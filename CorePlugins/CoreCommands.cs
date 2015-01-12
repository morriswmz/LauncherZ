using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using LauncherZLib.API;
using LauncherZLib.Event;
using LauncherZLib.Launcher;

namespace CorePlugins
{
    public class CoreCommands : IPlugin
    {
        private IPluginContext _pluginContext;
        private PerformanceCounter _cpuCounter;

        public void Activate(IPluginContext pluginContext)
        {
            _pluginContext = pluginContext;
            _pluginContext.EventBus.Register(this);
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        }

        public void Deactivate(IPluginContext pluginContext)
        {
            
        }

        public IEnumerable<LauncherData> Query(LauncherQuery query)
        {
            if (query.RawInput.ToLower() == "date")
            {
                return new List<LauncherData>()
                {
                    new LauncherData("Total CPU Usage", "", "LauncherZ|IconNetwork", 1.0,
                        new LauncherExtendedProperties(true, TickRate.Normal))
                };
            }
            return Enumerable.Empty<LauncherData>();
        }


        [SubscribeEvent]
        public void CommandItemTickHandler(LauncherTickEvent e)
        {
            var cpu = (int) _cpuCounter.NextValue();
            var bars = 20*cpu/100;
            var progress = new char[20];
            int i = 0;
            for (i = 0; i < bars; i++)
                progress[i] = '=';
            for (; i < 20; i++)
                progress[i] = ' ';
            e.LauncherData.Description = string.Format("{0}% \\[{1}\\]", cpu, new string(progress));
        }
    }
}
