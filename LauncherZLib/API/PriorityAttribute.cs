using System;

namespace LauncherZLib.API
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class PriorityAttribute : Attribute
    {

        private readonly double _priorityValue;

        public PriorityAttribute(double priority)
        {
            _priorityValue = priority;
        }

        public double Priority
        {
            get { return _priorityValue; }
        }

    }
}
