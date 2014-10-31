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
    [TaskProvider("TestProvider", Name = "TestProvider", Authors = "morriswmz", Version = "0.0.0.1")]
    [Description("For testing purpose only.")]
    
    public class TestProvider : ITaskProvider
    {
        [LauncherZEventHandler]
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

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Terminate()
        {
            throw new NotImplementedException();
        }
    }
}
