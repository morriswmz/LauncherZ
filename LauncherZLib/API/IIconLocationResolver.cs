using LauncherZLib.Icon;

namespace LauncherZLib.API
{
    public interface IIconLocationResolver
    {
        bool TryResolve(IconLocation location, out string path);
    }
}
