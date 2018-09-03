using System;
using System.Text;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Responses
{
    public static class ByteArrayExtensions
    {
        public static string ToNullTerminatedString(this byte[] buffer, int startIndex = 0, int bufferLength = 0)
        {
            bufferLength = bufferLength == 0 ? buffer.Length : bufferLength;
            int length = Array.FindIndex(buffer, startIndex, bufferLength, b => b == 0);
            return length != -1 ? Encoding.ASCII.GetString(buffer, startIndex, length) : string.Empty;
        }

    }
}