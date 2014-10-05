using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZ.Task
{

    public class TaskMetadata
    {
        public TaskMetadata(string providerId, bool executable, bool tickable)
        {
            ProviderId = providerId;
            Executable = executable;
            Tickable = tickable;
        }

        public string ProviderId { get; private set; }

        public bool Executable { get; private set; }

        public bool Tickable { get; private set; }
    }

}
