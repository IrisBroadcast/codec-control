using System;

namespace CodecControl.Client
{
    public class UnableToResolveAddressException : Exception
    {
        public UnableToResolveAddressException()
        {
        }

        public UnableToResolveAddressException(string message) : base(message)
        {
        }

        public UnableToResolveAddressException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}