using System;
using System.Windows.Threading;

namespace LauncherZLib.Utils
{
    /// <summary>
    /// Provides a simple implementation of
    /// <see cref="T:LauncherZLib.Utils.ITimerService"/> based on
    /// <see cref="T:System.Windows.Threading.DispatcherTimer"/>.
    /// NOT thread-safe.
    /// </summary>
    public class SimpleTimer : ITimerService
    {
        protected DispatcherTimer Timer;
        protected ActionNode HeadNode = new ActionNode { Id = -1 };
        protected DateTime LastTickTime;
        protected int IdCounter = 0;

        public SimpleTimer(Dispatcher dispatcher)
        {
            if (dispatcher == null)
                throw new ArgumentNullException("dispatcher");
            Timer = new DispatcherTimer(DispatcherPriority.Background, dispatcher);
            Timer.Stop();
            Timer.Tick += Timer_Tick;
        }

        public virtual TimeSpan MinimalResolution
        {
            get { return new TimeSpan(0, 0, 1); }
        }

        protected virtual void Timer_Tick(object sender, EventArgs e)
        {
            ActionNode prevNode = HeadNode;
            ActionNode node = HeadNode.Next;
            TimeSpan duration = DateTime.Now - LastTickTime;
            TimeSpan minRemaining = TimeSpan.MaxValue;
            while (node != null)
            {
                node.RemainingTime -= duration;
                if (node.RemainingTime.Ticks < 0)
                {
                    // expired
                    node.Action();
                    if (node.Interval.Ticks > 0)
                    {
                        // reset if possible
                        node.RemainingTime = node.Interval + node.RemainingTime;
                        prevNode = node;
                        node = node.Next;
                    }
                    else
                    {
                        // remove from list
                        prevNode.Next = node.Next;
                        node = node.Next;
                    }
                }
                else
                {
                    // not expired, check minimal remaining time and continue
                    if (node.RemainingTime.Ticks < minRemaining.Ticks)
                    {
                        minRemaining = node.RemainingTime;
                    }
                    prevNode = node;
                    node = node.Next;
                }
            }
            // check next minimal remaining time
            if (HeadNode.Next == null)
            {
                Timer.Stop();
                return;
            }
            Timer.Interval = minRemaining;
        }

        public virtual int SetTimeout(Action action, TimeSpan duration)
        {
            return InsertActionNode(action, duration, false);
        }

        public virtual void ClearTimeout(int id)
        {
            RemoveActionNode(id);
        }

        public virtual int SetInterval(Action action, TimeSpan interval)
        {
            return InsertActionNode(action, interval, true);
        }

        public virtual void ClearInterval(int id)
        {
            RemoveActionNode(id);
        }

        protected virtual int InsertActionNode(Action action, TimeSpan duration, bool repeat)
        {
            if (duration.Ticks < MinimalResolution.Ticks)
                duration = MinimalResolution;

            int newId = IdCounter++;
            var node = new ActionNode
            {
                Id = newId,
                StartTime = DateTime.Now,
                RemainingTime = duration,
                Interval = repeat ? duration : new TimeSpan(0),
                Action = action
            };

            if (HeadNode.Next == null)
            {
                // first one, start timer
                HeadNode.Next = node;
                Timer.Interval = HeadNode.Next.RemainingTime;
                Timer.Start();
            }
            else
            {
                // insert after head
                node.Next = HeadNode.Next;
                HeadNode.Next = node;
            }

            return newId;
        }

        protected virtual void RemoveActionNode(int id)
        {
            ActionNode prevNode = HeadNode;
            ActionNode node = HeadNode.Next;
            while (node != null)
            {
                if (node.Id == id)
                {
                    prevNode.Next = node.Next;
                    return;
                }
                prevNode = node;
                node = node.Next;
            }
            // check if last node is removed
            if (HeadNode.Next == null)
                Timer.Stop();
        }

        protected class ActionNode
        {
            public int Id { get; set; }
            public DateTime StartTime { get; set; }
            public TimeSpan RemainingTime { get; set; }
            public TimeSpan Interval { get; set; }
            public Action Action { get; set; }
            public ActionNode Next { get; set; }
        }

    }
}
