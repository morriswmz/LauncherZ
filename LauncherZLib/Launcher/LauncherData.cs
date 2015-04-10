using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LauncherZLib.Launcher
{
    public class LauncherData : INotifyPropertyChanged
    {

        private static readonly string DefaultFontFamily = "Global User Interface";

        private string _title = "";
        private string _description = "";
        private bool _isDescriptionVisible = true;
        private bool _isTitleVisible = true;
        private string _titleFont = DefaultFontFamily;
        private string _descriptionFont = DefaultFontFamily;
        private string _iconLocation = "";
        private readonly double _relevance;
        private readonly LauncherExtendedProperties _launcherEx;

        public event PropertyChangedEventHandler PropertyChanged;

        public LauncherData(string title, string description, string iconLocation, double relavance, LauncherExtendedProperties launcherEx)
        {

            _title = title;
            _description = description;
            _iconLocation = iconLocation;
            _relevance = relavance;
            _launcherEx = launcherEx;
        }

        public LauncherData(string title, string description, string iconLocation, double relavance)
            : this(title, description, iconLocation, relavance, new LauncherExtendedProperties(false))
        { }

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
        public bool IsTitleVisible
        {
            get { return _isTitleVisible; }
            set
            {
                if (_isTitleVisible != value)
                {
                    _isTitleVisible = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// Gets or sets the description visibility.
        /// Triggers PropertyChanged event.
        /// </summary>
        public bool IsDescriptionVisible
        {
            get { return _isDescriptionVisible; }
            set
            {
                if (_isDescriptionVisible != value)
                {
                    _isDescriptionVisible = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// Gets or sets the font family of the title.
        /// </summary>
        public string TitleFont
        {
            get { return _titleFont; }
            set
            {
                string newFont = value ?? DefaultFontFamily;
                if (!_titleFont.Equals(newFont, StringComparison.OrdinalIgnoreCase))
                {
                    _titleFont = newFont;
                    RaisePropertyChangedEvent();
                }
            }
        }

        /// <summary>
        /// Gets or sets the font family of the description.
        /// </summary>
        public string DescriptionFont
        {
            get { return _descriptionFont; }
            set
            {
                string newFont = value ?? DefaultFontFamily;
                if (!_descriptionFont.Equals(newFont, StringComparison.OrdinalIgnoreCase))
                {
                    _descriptionFont = newFont;
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
        /// Gets or sets the icon location in string form.
        /// The value of this property will be used to construct
        /// <see cref="Icon.IconLocation"/>.
        /// Triggers PropertyChanged event.
        /// </summary>
        public string IconLocation { 
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
        /// Gets or sets the plugin id associated with this command.
        /// Internal usage only.
        /// </summary>
        public string PluginId { get; internal set; }

        /// <summary>
        /// Gets the extended properties of this command.
        /// </summary>
        public LauncherExtendedProperties ExtendedProperties
        {
            get { return _launcherEx; }
        }
        
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
            return string.Format("[{0}]{1}", Relevance, Title);
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
                "LauncherZ|IconBlank", 0.0, new LauncherExtendedProperties(false)
                )
        {
            DescriptionFont = "Seoge UI";
        }

        public LauncherDataDesignTime() : this(1) { }
    }

    

}
