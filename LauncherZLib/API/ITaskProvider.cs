using System.Collections.Generic;
using System.Globalization;
using LauncherZLib.LauncherTask;

namespace LauncherZLib.API
{
    public interface ITaskProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        string GetLocalizedName(CultureInfo cultureInfo);

        void Initialize(IEventBus eventBus);

        void Terminate();

        IEnumerable<TaskData> Query(TaskQuery query);

        bool Execute(TaskData taskData);
        

    }
}
