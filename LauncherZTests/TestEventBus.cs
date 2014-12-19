using System;
using LauncherZLib.Event;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LauncherZTests
{
    [TestClass]
    public class TestEventBus
    {

        private EventBus eventBus = new EventBus();

        [TestMethod]
        public void TestEvent()
        {
            var e = new DummyEvent(0);
            var h = new DummyEventHandler();
            eventBus.Register(this);
            eventBus.Register(h);
            eventBus.Post(e);
            Assert.AreEqual(10, e.N);
            Assert.IsTrue(e.IsDefaultPrevented);
        }
    }

    public class DummyEvent : EventBase
    {
        public int N { get; set; }

        public DummyEvent(int n)
        {
            N = n;
        }
    }

    class DummyEventHandler
    {
        [SubscribeEvent]
        public void HandleEventA(DummyEvent e)
        {
            e.N++;
        }

        public void HandleEventB(DummyEvent e)
        {
            e.N--;
        }

        [SubscribeEvent]
        private void HandleEventC(DummyEvent e)
        {
            e.N *= 10;
            e.PreventDefault();
        }
    }

}
