using System;
using System.Collections.Generic;
using System.Globalization;
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
                    result.Add(new LauncherData("TestItem" + i, "[Hover] to generate a random number.", "",
                        1.0 - (double) i/5.0, new LauncherExtendedProperties(false, TickRate.Slow)));
                }
            }

            return result;
        }

        [SubscribeEvent]
        public void LauncherTickHandler(LauncherTickEvent e)
        {
            e.LauncherData.Description = string.Format("Number [{0}]", _random.NextDouble());
        }

        [SubscribeEvent]
        public void LauncherSelectedHandler(LauncherSelectedEvent e)
        {
            e.LauncherData.ExtendedProperties.Tickable = true;
            e.LauncherData.Description = "Waiting for random number generator...";
        }

        [SubscribeEvent]
        public void LauncherDeselectedHandler(LauncherDeselectedEvent e)
        {
            e.LauncherData.ExtendedProperties.Tickable = false;
            e.LauncherData.Description = "[Hover] to generate a random number.";
        }

        [SubscribeEvent]
        public void LauncherExecutedHanlder(LauncherExecutedEvent e)
        {
            e.PreventDefault();
        }

    }
}
