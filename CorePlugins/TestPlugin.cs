using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LauncherZLib.API;
using LauncherZLib.Event;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin;

namespace CorePlugins
{
    public class TestPlugin : IPluginAsync
    {

        private IPluginContext _context;
        private Random _random = new Random();

        public event EventHandler<AsyncUpdateEventArgs> AsyncUpdate;

        public void Activate(IPluginContext pluginContext)
        {
            _context = pluginContext;
            _context.EventBus.Register(this);
        }

        public void Deactivate(IPluginContext pluginContext)
        {
            _context = null;
        }

        public IEnumerable<LauncherData> Query(LauncherQuery query)
        {
            
            var result = new List<LauncherData>();

            if (!string.IsNullOrEmpty(query.RawInput))
            {
                for (var i = 0; i < 4; i++)
                {
                    result.Add(new LauncherData("TestItem" + i, "", "",
                        1.0 - (double) i/5.0, new LauncherExtendedProperties(true, TickRate.Slow)));
                }
            }

            return result;
        }

        [SubscribeEvent]
        public void CommandItemTickHandler(LauncherTickEvent e)
        {
            e.LauncherData.Description = string.Format("Number [{0}]", _random.NextDouble());
        }

        [SubscribeEvent]
        public void CommandItemSelectedHandler(LauncherSelectedEvent e)
        {
            e.LauncherData.ExtendedProperties.Tickable = true;
        }

        [SubscribeEvent]
        public void CommandItemDeselectedHandler(LauncherDeselectedEvent e)
        {
            e.LauncherData.ExtendedProperties.Tickable = false;
        }

    }
}
