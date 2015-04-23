using System;
using System.Collections.Generic;
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
        protected Dispatcher CurrentDispatcher;
        protected ActionNode HeadNode = new ActionNode { Id = -1 };
        protected DateTime LastTickTime;
        protected int IdCounter = 0;
        protected bool ActionGroupsSorted = false;
        protected readonly List<ActionGroup> ActionGroups = new List<ActionGroup>();

        public SimpleTimer(Dispatcher dispatcher)
        {
            if (dispatcher == null)
                throw new ArgumentNullException("dispatcher");
            CurrentDispatcher = dispatcher;
        }

        public virtual TimeSpan MinimalResolution
        {
            get { return new TimeSpan(0, 0, 1); }
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

        protected virtual void TickGroup(ActionGroup group)
        {
            var minRemainingTime = TimeSpan.MaxValue;
            ActionNode prevNode = group.Head;
            ActionNode curNode = group.Head.Next;
            var now = DateTime.Now;
            TimeSpan realDuration = now - group.LastTickTime;
            group.LastTickTime = now;
            while (curNode != null)
            {
                curNode.RemainingTime -= realDuration;
                // check if time is up
                if (curNode.RemainingTime.Ticks > 0)
                {
                    if (curNode.RemainingTime < minRemainingTime)
                        minRemainingTime = curNode.RemainingTime;
                    prevNode = curNode;
                    curNode = curNode.Next;
                    continue;
                }
                // time's up !
                if (curNode.Interval.Ticks > 0L)
                {
                    // fire all repeating actions
                    while (curNode.RemainingTime.Ticks <= 0)
                    {
                        curNode.RemainingTime = curNode.Interval + curNode.RemainingTime;
                        curNode.Action();
                    }
                    // reset and update
                    if (curNode.RemainingTime < minRemainingTime)
                        minRemainingTime = curNode.RemainingTime;
                    // next
                    prevNode = curNode;
                    curNode = curNode.Next;
                }
                else
                {
                    // single time action
                    curNode.Action();
                    // remove
                    prevNode.Next = curNode.Next;
                    curNode = curNode.Next;
                }
            }
            if (group.Head.Next == null)
            {
                // all actions done
                RemoveActionGroup(group);
                return;
            }
            // update timer
            TimeSpan newInterval = minRemainingTime - (DateTime.Now - now);
            group.Timer.Interval = newInterval.Ticks > 0 ? newInterval : new TimeSpan(0L);
            group.Timer.Start();
            ActionGroupsSorted = false;
        }

        protected virtual int InsertActionNode(Action action, TimeSpan duration, bool repeat)
        {
            if (duration.Ticks < MinimalResolution.Ticks)
                duration = MinimalResolution;

            // create new node
            int newId = IdCounter++;
            var node = new ActionNode
            {
                Id = newId,
                StartTime = DateTime.Now,
                RemainingTime = duration,
                Interval = repeat ? duration : new TimeSpan(0),
                Action = action
            };

            if (ActionGroups.Count == 0)
            {
                // first time
                ActionGroups.Add(CreateAndStartNewActionGroup(node));
            }
            else
            {
                // find appropriate group
                ActionGroup optActionGroup = FindOptimalActionGroup(node);
                if (optActionGroup == null)
                {
                    // new group
                    ActionGroups.Add(CreateAndStartNewActionGroup(node));
                }
                else
                {
                    // insert into existing group
                    ActionNode optGroupHead = optActionGroup.Head;
                    node.Next = optGroupHead.Next;
                    optGroupHead.Next = node;
                }
            }

            return newId;
        }

        protected virtual void RemoveActionNode(int id)
        {
            foreach (var group in ActionGroups)
            {
                ActionNode prevNode = group.Head;
                ActionNode node = prevNode.Next;
                while (node != null)
                {
                    if (node.Id == id)
                    {
                        prevNode.Next = node.Next;
                        // check if last node is removed
                        if (group.Head.Next == null)
                        {
                            RemoveActionGroup(group);
                        }
                        return;
                    }
                    prevNode = node;
                    node = node.Next;
                }
            }
        }

        protected virtual ActionGroup FindOptimalActionGroup(ActionNode node)
        {
            if (!ActionGroupsSorted)
            {
                // sort by descending order
                ActionGroups.Sort((x, y) => y.Timer.Interval.CompareTo(x.Timer.Interval));
            }
            int i = 0, n = ActionGroups.Count;
            for (; i < n; i++)
            {
                if (node.RemainingTime > ActionGroups[i].Timer.Interval)
                    break;
            }
            return i == n ? null : ActionGroups[i];
        }

        protected virtual void RemoveActionGroup(ActionGroup group)
        {
            group.Timer.Tick -= group.TickHandler;
            group.Timer.Stop();
            ActionGroups.Remove(group);
        }

        protected ActionGroup CreateAndStartNewActionGroup(ActionNode firstNode)
        {
            var timer = new DispatcherTimer(DispatcherPriority.Background, CurrentDispatcher)
            {
                Interval = GetSuggestedInitialInterval(firstNode.RemainingTime)
            };
            var newGroup = new ActionGroup(timer);
            newGroup.Head.Next = firstNode;
            newGroup.LastTickTime = DateTime.Now;
            newGroup.TickHandler = (sender, args) => TickGroup(newGroup);
            timer.Tick += newGroup.TickHandler;
            timer.Start();
            return newGroup;
        }

        protected virtual TimeSpan GetSuggestedInitialInterval(TimeSpan remainingTime)
        {
            long ticks = remainingTime.Ticks/2;
            ticks = MinimalResolution.Ticks*(ticks/MinimalResolution.Ticks);
            return ticks > MinimalResolution.Ticks ? new TimeSpan(ticks) : remainingTime;
        }

        protected class ActionGroup
        {
            public ActionNode Head { get; private set; }
            public DispatcherTimer Timer { get; private set; }
            public EventHandler TickHandler { get; set; }
            public DateTime LastTickTime { get; set; }

            public ActionGroup(DispatcherTimer timer)
            {
                Head = new ActionNode();
                Timer = timer;
            }
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
