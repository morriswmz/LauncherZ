namespace LauncherZLib.Icon
{
    public interface IIconProviderRegisterer
    {
        bool IsDomainRegistered(string domain);
        
        void RegisterIconProvider(IIconProvider iconProvider);
    }
}
