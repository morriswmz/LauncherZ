using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LauncherZLib.Event;
using LauncherZLib.Event.Plugin;
using LauncherZLib.Launcher;
using LauncherZLib.Plugin;
using LauncherZLib.Plugin.Service;

namespace CorePlugins
{

    public class TestPlugin : IPlugin
    {

        private IEventBus _eventBus;
        private Random _random = new Random();

        public void Activate(IExtendedServiceProvider serviceProvider)
        {
            _eventBus = serviceProvider.GetService<IEventBus>();
            _eventBus.Register(this);
        }

        public void Deactivate(IExtendedServiceProvider serviceProvider)
        {

        }

        public IEnumerable<LauncherData> Query(LauncherQuery query)
        {
            
            /*var result = new List<LauncherData>();
            var icons = new string[]
            {
                @"C:\ShipID.txt",
                @"D:\list.txt",
                @"D:\SkyDrive\Private Photo\CA\Scenery\RIMG1550.JPG",
                @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Adobe Reader XI.lnk"
            };
            if (!string.IsNullOrEmpty(query.OriginalInput))
            {
                for (var i = 0; i < 4; i++)
                {
                    result.Add(new LauncherData("TestItem" + i, "[Hover] to generate a random number.", icons[i],
                        1.0 - (double) i/5.0, new LauncherExtendedProperties(false, TickRate.Slow)));
                }
            }*/
            Task.Run(() =>
            {
                Thread.Sleep(1000);
                _eventBus.Post(new QueryResultUpdateEvent(query.QueryId, new LauncherData[]
                {
                    new LauncherData(
                        "Test", string.Format("[Delayed by 1000ms] {0}", DateTime.Now),
                        "LauncherZ|IconGear", 1.0)
                }));
            });

            return Enumerable.Empty<LauncherData>();
        }

        public PostLaunchAction Launch(LauncherData launcherData)
        {
            return PostLaunchAction.Default;
        }

        /*
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
        }*/

    }
}
