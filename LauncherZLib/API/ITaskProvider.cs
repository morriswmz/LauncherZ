using System.Collections.Generic;
using LauncherZLib.Task;

namespace LauncherZLib.API
{
    public interface ITaskProvider
    {

        void Initialize();

        void Terminate();

        IEnumerable<TaskData> Query(TaskQuery query);

        bool Execute(TaskData taskData);
        

    }
}
