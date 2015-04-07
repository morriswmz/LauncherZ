using System;

namespace LauncherZLib.Utils
{
    /// <summary>
    /// Provides a simple timer service.
    /// </summary>
    public interface ITimerService
    {
        /// <summary>
        /// Gets the minimal resolution of the timer service.
        /// </summary>
        TimeSpan MinimalResolution { get; }

        /// <summary>
        /// <para>Performs an action after specific duration.</para>
        /// <para>This method will return immediately.</para>
        /// </summary>
        /// <param name="action">Action to be performed.</param>
        /// <param name="duration">
        /// <para>Duration before the action is performed.</para>
        /// <para>
        /// This value cannot be smaller than
        /// <see cref="P:LauncherZLib.Utils.ITimerService.MinimalResolution"/>.
        /// </para>
        /// </param>
        /// <returns>Id of the of the scheduled action.</returns>
        /// <remarks>
        /// Timing may be inaccurate if given interval is not a multiply of
        /// <see cref="P:LauncherZLib.Utils.ITimerService.MinimalResolution"/>.
        /// </remarks>
        int SetTimeout(Action action, TimeSpan duration);
        
        /// <summary>
        /// Clears an scheduled action.
        /// </summary>
        /// <param name="id">
        /// Id of the of the scheduled action. Returned by
        /// <see cref="M:LauncherZLib.Utils.ITimerService.SetTimeout"/>
        /// </param>
        void ClearTimeout(int id);
        
        /// <summary>
        /// <para>Performs an action repeatly for every specified interval.</para>
        /// <para>This method will return immediately.</para>
        /// </summary>
        /// <param name="action">Action to be performed.</param>
        /// <param name="interval">
        /// <para>Interval between actions.</para>
        /// This value cannot be smaller than
        /// <see cref="P:LauncherZLib.Utils.ITimerService.MinimalResolution"/>.
        /// </param>
        /// <returns>Id of the of the scheduled repeating action.</returns>
        /// <remarks>
        /// Timing may be inaccurate if given interval is not a multiply of
        /// <see cref="P:LauncherZLib.Utils.ITimerService.MinimalResolution"/>.
        /// </remarks>
        int SetInterval(Action action, TimeSpan interval);
        
        /// <summary>
        /// Clears an scheduled repeating action.
        /// </summary>
        /// <param name="id">
        /// Id of the of the scheduled repeating action. Returned by
        /// <see cref="M:LauncherZLib.Utils.ITimerService.SetInterval"/>
        /// </param>
        void ClearInterval(int id);
    
    }
}
