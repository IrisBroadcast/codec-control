using System;

namespace CodecControl.Client.Exceptions
{
    public abstract class CodecControlException : ApplicationException
    {

    }

    public class CodecApiNotFoundException : ApplicationException
    {
        public CodecApiNotFoundException()
        {
        }

        public CodecApiNotFoundException(string message) : base(message)
        {
        }

        public CodecApiNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}