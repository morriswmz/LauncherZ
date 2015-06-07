namespace LauncherZLib.Launcher
{
    public sealed class PostLaunchAction
    {

        /// <summary>
        /// Default action. Hides window.
        /// </summary>
        public static readonly PostLaunchAction Default = new PostLaunchAction(true, false, false, "");
        /// <summary>
        /// Does nothing.
        /// </summary>
        public static readonly PostLaunchAction DoNothing = new PostLaunchAction(false, false, false, "");
        /// <summary>
        /// Resets input and enters standalone mode. Window is kept shown.
        /// </summary>
        public static readonly PostLaunchAction ResetInputAndEnableStandaloneMode = new PostLaunchAction(false, true, true, "");
        /// <summary>
        /// Resets input and exits standalone mode. Window is kept shown.
        /// </summary>
        public static readonly PostLaunchAction ResetInputAndDisableStandaloneMode = new PostLaunchAction(false, true, false, "");

        public PostLaunchAction(bool hideWindow, bool modifyInput, bool enableStandaloneMode, string modifiedInput)
        {
            HideWindow = hideWindow;
            ModifyInput = modifyInput;
            EnableStandaloneMode = enableStandaloneMode;
            ModifiedInput = modifiedInput;
        }

        /// <summary>
        /// Hides the window and resets everything. If set to true, all other properties will be ignored.
        /// </summary>
        public bool HideWindow { get; set; }
        
        /// <summary>
        /// Set to true to enable modification of the input.
        /// </summary>
        public bool ModifyInput { get; set; }
        
        /// <summary>
        /// Set to true to enter standalone mode, where following queries will be handled by this plugin
        /// only.
        /// </summary>
        public bool EnableStandaloneMode { get; set; }
        
        /// <summary>
        /// Modfied input. Effective when <see cref="P:LauncherZLib.Launcher.PostLaunchAction.ModifyInput"/>
        /// is true.
        /// </summary>
        public string ModifiedInput { get; set; }

    }
}
