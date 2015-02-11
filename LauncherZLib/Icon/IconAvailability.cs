namespace LauncherZLib.Icon
{
    /// <summary>
    /// Describes the availability of an icon.
    /// </summary>
    public enum IconAvailability
    {
        /// <summary>
        /// Not available.
        /// </summary>
        NotAvailable = 0,

        /// <summary>
        /// Available for immediate usage.
        /// </summary>
        Available,

        /// <summary>
        /// Available, but some work needs to be done first, such as loading from disk
        /// and download from the Internet.
        /// </summary>
        AvailableLater
    }
}
