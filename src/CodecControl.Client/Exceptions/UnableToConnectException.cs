using System;

namespace CodecControl.Client
{
    public class UnableToConnectException : Exception
    {
        public UnableToConnectException()
        {
        }

        public UnableToConnectException(string message) : base(message)
        {
        }

        public UnableToConnectException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}