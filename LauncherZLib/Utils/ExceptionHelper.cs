using System;

namespace LauncherZLib.Utils
{
    public enum ExceptionTypes
    {
        ModifyReadonlyCollection
    }

    public static class ExceptionHelper
    {
        
        public static Exception Create(ExceptionTypes type)
        {
            switch (type)
            {
                case ExceptionTypes.ModifyReadonlyCollection:
                    return new NotSupportedException("Collection is read-only.");
                default:
                    return new Exception();
            }
        }
    }
}
