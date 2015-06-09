namespace LauncherZLib.Launcher
{
    public sealed class PostLaunchAction
    {

        /// <summary>
        /// Default action. Hides window.
        /// </summary>
        public static readonly PostLaunchAction Default = new PostLaunchAction(true, false, false, false, "");
        /// <summary>
        /// Does nothing.
        /// </summary>
        public static readonly PostLaunchAction DoNothing = new PostLaunchAction(false, false, false, false, "");

        public PostLaunchAction(bool hideWindow, bool modifyInput, bool lockUserInput, bool enableStandaloneMode, string modifiedInput)
        {
            HideWindow = hideWindow;
            ModifyInput = modifyInput;
            LockUserInput = lockUserInput;
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
        /// Set to true to lock the input so user cannot modify the query.
        /// Setting to false will unlock the user input.
        /// </summary>
        public bool LockUserInput { get; set; }
        
        /// <summary>
        /// Set to true to enter standalone mode, where following queries will be handled by this plugin
        /// only. Setting to false to disable standalone mode.
        /// </summary>
        public bool EnableStandaloneMode { get; set; }
        
        /// <summary>
        /// Gets or sets the modfied input. Effective when <see cref="P:LauncherZLib.Launcher.PostLaunchAction.ModifyInput"/>
        /// is true.
        /// </summary>
        public string ModifiedInput { get; set; }

    }
}
