using System;
using System.ComponentModel;

namespace LauncherZLib.Task
{
    public class TaskData : INotifyPropertyChanged
    {

        private string _title = "";
        private string _description = "";
        private bool _isDescriptionVisible = true;
        private bool _isTitleVisible = true;
        private readonly TaskMetadata _taskMetadata;

        public event PropertyChangedEventHandler PropertyChanged;

        public TaskData(string title, string description, string iconLocation, TaskMetadata taskMetadata)
        {
            _title = title;
            _description = description;
            IconLocation = iconLocation;
            _taskMetadata = taskMetadata;
        }

        #region Properties

        public string Title
        {
            get { return _title; }
            set
            {
                if (_title == value) return;
                _title = value;
                DispatchPropertyChangedEvent(new PropertyChangedEventArgs("Title"));
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description == value) return;
                _description = value;
                DispatchPropertyChangedEvent(new PropertyChangedEventArgs("Description"));
            }
        }

        public bool IsTitleVisible
        {
            get { return _isTitleVisible; }
            set
            {
                if (_isTitleVisible == value) return;
                _isTitleVisible = value;
                DispatchPropertyChangedEvent(new PropertyChangedEventArgs("IsTitleVisible"));
            }
        }

        public bool IsDescriptionVisible
        {
            get { return _isDescriptionVisible; }
            set { _isDescriptionVisible = value; }
        }

        public string IconLocation { 
            get;
            set;
        }

        public TaskMetadata MetaData
        {
            get { return _taskMetadata; }
        }

        

        #endregion

        protected void DispatchPropertyChangedEvent(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }


    public sealed class TaskDataDesignTime : TaskData
    {
        public TaskDataDesignTime(int index)
            : base(
                string.Format("Design [time] task {0}", index),
                "This is shown in _design mode_ only. " +
                "[Highlights] are ~supported~.\n" +
                "Creation Time: " + DateTime.Now.ToLongTimeString(),
                "Internal:IconGear", new TaskMetadata("", false, false)
            ) {}

        public TaskDataDesignTime() : this(1) { }
    }
}
