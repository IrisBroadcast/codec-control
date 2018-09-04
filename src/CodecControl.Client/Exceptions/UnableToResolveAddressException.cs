using System;

namespace CodecControl.Client.Exceptions
{
    public class UnableToResolveAddressException : ApplicationException
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