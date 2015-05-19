namespace LauncherZLib.Icon
{
    /// <summary>
    /// Provides registration for icon providers.
    /// </summary>
    public interface IIconProviderRegistry
    {
        /// <summary>
        /// Checks if specified domain has any icon provider registered.
        /// </summary>
        /// <param name="domain"></param>
        /// <returns>
        /// True if there exists any corresponding icon provider.
        /// False if not registered or domain is invalid.
        /// </returns>
        bool IsDomainRegistered(string domain);
        
        /// <summary>
        /// <para>Registers an icon provider.</para>
        /// <para>
        /// If an icon provider is registered again for different domains.
        /// Existing registration will be removed first.
        /// </para>
        /// </summary>
        /// <param name="domains">
        /// Domains supported by the icon providers.
        /// Multiple domains are separated by commas.
        /// </param>
        /// <param name="iconProvider">Icon provider to be registered.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// Thrown when any of the arguments is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        /// Thrown when domains is empty or domain names are invalid. 
        /// </exception>
        void RegisterIconProvider(string domains, IIconProvider iconProvider);

        /// <summary>
        /// <para>Removes an icon provider from the registry.</para>
        /// <para>Does nothing if given icon provider is not registered.</para>
        /// </summary>
        /// <param name="iconProvider">Icon provider to be removed.</param>
        void UnregisterIconProvider(IIconProvider iconProvider);
    }
}
