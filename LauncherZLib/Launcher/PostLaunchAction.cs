using System;

namespace LauncherZLib.Launcher
{
    [Flags]
    public enum PostLaunchActionType
    {
        /// <summary>
        /// Hides window, overrides all other actions, and resets query.
        /// </summary>
        HideWindow = 0x80,
        /// <summary>
        /// Modifies query uri, useful for special actions requiring a non-standard query uri.
        /// You should change the query uri back to a standarded one upon completing the special action.
        /// Overrides modify input action.
        /// </summary>
        ModifyUri = 0x40,
        /// <summary>
        /// Modifies the input only, other query parameters will remain unchanged.
        /// </summary>
        ModifyInput = 0x20,
        /// <summary>
        /// Modifies hint text.
        /// </summary>
        ModifyHint = 0x02,
        /// <summary>
        /// Locks user input.
        /// </summary>
        LockInput = 0x01,
        /// <summary>
        /// Do nothing, zero flag.
        /// </summary>
        None = 0x00
    }

    public sealed class PostLaunchAction
    {

        /// <summary>
        /// Default action. Hides window.
        /// </summary>
        public static readonly PostLaunchAction Default = new PostLaunchAction(PostLaunchActionType.HideWindow, "", "", "");
        /// <summary>
        /// Does nothing.
        /// </summary>
        public static readonly PostLaunchAction DoNothing = new PostLaunchAction();

        public PostLaunchAction() : this(PostLaunchActionType.None, "", "", "")
        {
        }

        public PostLaunchAction(PostLaunchActionType actionType, string modifiedInput, string modifiedUri, string modifiedHint)
        {
            ActionType = actionType;
            ModifiedInput = modifiedInput;
            ModifiedUri = modifiedUri;
            ModifiedHint = modifiedHint;
        }

        /// <summary>
        /// Gets or sets the action type.
        /// </summary>
        public PostLaunchActionType ActionType { get; set; }

        /// <summary>
        /// Gets or sets the modified uri.
        /// </summary>
        /// <see cref="T:LauncherZLib.Launcher.PostLaunchActionType"/>
        public string ModifiedUri { get; set; }

        /// <summary>
        /// Gets or sets the modified input.
        /// </summary>
        /// <see cref="T:LauncherZLib.Launcher.PostLaunchActionType"/>
        public string ModifiedInput { get; set; }

        /// <summary>
        /// Gets or sets the modified hint, shown above the input.
        /// Set to null or empty to reset. 
        /// </summary>
        /// <see cref="T:LauncherZLib.Launcher.PostLaunchActionType"/>
        public string ModifiedHint { get; set; }
        
    }

}
