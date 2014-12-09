using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.API;
using LauncherZLib.Task;

namespace TestProvider
{
    public class TestProvider : ITaskProvider
    {
        public void FooEventHandler(object sender, EventArgs e)
        {
            
        }

        public string ProviderId { get; private set; }

        public IEnumerable<TaskData> Query(TaskQuery query)
        {
            throw new NotImplementedException();
        }

        public bool Execute(TaskData taskData)
        {
            throw new NotImplementedException();
        }

        public void Initialize(IEventBus eventBus)
        {
            throw new NotImplementedException();
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }
    }
}
