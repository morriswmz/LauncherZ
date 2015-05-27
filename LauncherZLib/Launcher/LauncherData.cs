using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LauncherZLib.Launcher
{
    /// <summary>
    /// Data model for a launcher.
    /// You may extend this class if you wish to store additional data.
    /// </summary>
    public class LauncherData : INotifyPropertyChanged
    {

        private static readonly string DefaultFontFamily = "Global User Interface";
        private static long _uidCounter = 0;

        private string _title = "";
        private string _description = "";
        private bool _usesDescription = true;
        private bool _usesTitle = true;
        private string _iconLocation = "";
        private readonly double _relevance;
        private readonly long _uniqueId;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Creates a new LauncherData with specified relevance.
        /// </summary>
        /// <param name="relavance"></param>
        public LauncherData(double relavance) : this("Untitled launcher", "No description", "", relavance)
        {

        }

        /// <summary>
        /// Creates a new LauncherData with details.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="iconLocation"></param>
        /// <param name="relavance"></param>
        public LauncherData(string title, string description, string iconLocation, double relavance)
        {
            _title = title;
            _description = description;
            _iconLocation = iconLocation;
            _relevance = relavance;
            Tickable = false;
            CurrentTickRate = TickRate.Normal;
            _uniqueId = Interlocked.Increment(ref _uidCounter);
        }

        #region Properties

        /// <summary>
        /// Gets or sets the title.
        /// Triggers PropertyChanged event.
        /// </summary>
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title != value)
                {
                    _title = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// Gets or sets the description, shown below the title.
        /// Triggers PropertyChanged event.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set
            {
                if (_description != value)
                {
                    _description = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// Gets or sets the title visibility.
        /// Triggers PropertyChanged event.
        /// </summary>
        public bool UsesTitle
        {
            get { return _usesTitle; }
            set
            {
                if (_usesTitle != value)
                {
                    _usesTitle = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// Gets or sets the description visibility.
        /// Triggers PropertyChanged event.
        /// </summary>
        public bool UsesDescription
        {
            get { return _usesDescription; }
            set
            {
                if (_usesDescription != value)
                {
                    _usesDescription = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// Gets or sets the icon location in string form.
        /// The value of this property will be used to construct
        /// <see cref="Icon.IconLocation"/>.
        /// Triggers PropertyChanged event.
        /// </summary>
        public string IconLocation
        {
            get { return _iconLocation; }
            set
            {
                if (_iconLocation != value)
                {
                    _iconLocation = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// Gets the relevance level, a value between 0.0 and 1.0.
        /// 0.0 implies complete irrelevance.
        /// </summary>
        public double Relevance
        {
            get { return _relevance; }
        }
        
        /// <summary>
        /// Gets or sets whether this launcher is tickable.
        /// </summary>
        public bool Tickable { get; set; }

        /// <summary>
        /// Gets or sets the tick rate.
        /// </summary>
        public TickRate CurrentTickRate { get; set; }

        /// <summary>
        /// Gets or sets the string data. You may use this property to store extra string data
        /// without extending this class.
        /// </summary>
        public string StringData { get; set; }

        /// <summary>
        /// Gets the unique id associated with the metadata.
        /// Each instance will be assigned a unique id.
        /// </summary>
        public long UniqueId { get { return _uniqueId;  } }

        #endregion

        protected void RaisePropertyChangedEvent([CallerMemberName] string propName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propName));
            }
        }

        public override string ToString()
        {
            return string.Format("{0}|{1}", Relevance, Title);
        }
    }

    /// <summary>
    /// Sample command used for design mode display.
    /// </summary>
    public sealed class LauncherDataDesignTime : LauncherData
    {
        public LauncherDataDesignTime(int index)
            : base(
                string.Format("Design [time] title {0}", index),
                "This is shown in _design mode_ only. " +
                "[Highlights] are ~supported~.\n" +
                "They are [[[~_Stackable_~]]]. " +
                "Creation Time: " + DateTime.Now.ToLongTimeString(),
                "LauncherZ|IconBlank", 0.0
                )
        {
            
        }

        public LauncherDataDesignTime() : this(1) { }
    }

    

}
