using System;
using System.Runtime.Serialization;

namespace LauncherZLib.Event
{
    /// <summary>
    /// The exception that is thrown when a method marked with SubscribeEventAttribute is not
    /// a valid event handler.
    /// </summary>
    [Serializable]
    public class InvalidHandlerException : Exception
    {
        public InvalidHandlerException()
        {
        }

        public InvalidHandlerException(string message) : base(message)
        {
        }

        public InvalidHandlerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidHandlerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
