using System;
using System.Windows.Threading;

namespace LauncherZLib.Utils
{
    public class SimpleScheduler
    {
        protected DispatcherTimer Timer;
        protected int Id = 0;

        public SimpleScheduler(DispatcherTimer timer)
        {
            if (timer == null)
                throw new ArgumentNullException("timer");
            Timer = timer;
        }

        public virtual int SetTimeout(Action action, int seconds)
        {
            
        }

        public virtual void ClearTimeout(int id)
        {
            
        }

        public virtual int SetInterval(Action action, int seconds)
        {
            
        }

        public virtual void ClearInterval(int id)
        {
            
        }

        protected class ActionNode
        {
            public int Id { get; set; }
            public DateTime StartTime { get; set; }
            public int RemainingTime { get; set; }
            public bool Repeat { get; set; }
            public Action Action { get; set; }
            public ActionNode Next { get; set; }
        }

    }
}
