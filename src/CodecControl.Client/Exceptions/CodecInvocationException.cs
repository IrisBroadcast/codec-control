using System;

namespace CodecControl.Client.Exceptions
{
    public class CodecInvocationException : ApplicationException
    {
        public CodecInvocationException(string message = "Operation failed") : base(message)
        {
            
        }
    }
}