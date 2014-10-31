namespace LauncherZLib.Task
{
    public class TaskEventDistributor
    {

        private static readonly TaskEventDistributor InstanceStorage = new TaskEventDistributor();

        public TaskEventDistributor Instance
        {
            get { return InstanceStorage; }
        }
        

    }
}
