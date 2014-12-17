using System.Collections.Generic;
using LauncherZLib.LauncherTask;

namespace LauncherZLib.API
{
    public interface ITaskProvider
    {

        void Initialize(IEventBus eventBus);

        void Terminate();

        IEnumerable<TaskData> Query(TaskQuery query);

        bool Execute(TaskData taskData);
        

    }
}
