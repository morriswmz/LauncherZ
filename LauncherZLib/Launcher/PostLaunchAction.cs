namespace LauncherZLib.Launcher
{
    public sealed class PostLaunchAction
    {

        /// <summary>
        /// Default action. Hides window.
        /// </summary>
        public static readonly PostLaunchAction Default = new PostLaunchAction(true, false, false, "");
        /// <summary>
        /// Resets input and enters exclusive mode. Window is kept shown.
        /// </summary>
        public static readonly PostLaunchAction ResetInputAndEnterExclusiveMode = new PostLaunchAction(false, true, true, "");
        /// <summary>
        /// Resets input and exits exclusive mode. Window is kept shown.
        /// </summary>
        public static readonly PostLaunchAction ResetInputAndExitExclusiveMode = new PostLaunchAction(false, true, false, "");

        public PostLaunchAction(bool hideWindow, bool modifyInput, bool enterExclusiveMode, string modifiedInput)
        {
            HideWindow = hideWindow;
            ModifyInput = modifyInput;
            EnterExclusiveMode = enterExclusiveMode;
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
        /// Set to true to enter exclusive mode, where following queries will be handled exclusively by
        /// this plugin.
        /// </summary>
        public bool EnterExclusiveMode { get; set; }
        
        /// <summary>
        /// Modfied input. Effective when <see cref="P:LauncherZLib.Launcher.PostLaunchAction.ModifyInput"/>
        /// is true.
        /// </summary>
        public string ModifiedInput { get; set; }

    }
}
