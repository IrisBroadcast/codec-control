using System;
using System.Net.Sockets;
using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Responses
{
    public abstract class IkusNetStatusResponseBase
    {
        protected static byte[] GetResponseBytes(SocketProxy socket, Command expectedCommand, int expectedResponseLength)
        {
            // Read fixed part of header
            var buffer = new byte[8];
            socket.Receive(buffer);
            var command = (Command) ConvertHelper.DecodeUInt(buffer, 0);
            var length = (int) ConvertHelper.DecodeUInt(buffer, 4);

            // Read variable part of header
            var result = new byte[length];
            socket.Receive(result);

            if (command != expectedCommand || length != expectedResponseLength)
            {
                throw new Exception(string.Format(
                    "Invalid response from codec. Command was {0} and length {1}. Expected {2} with length {3}",
                    command, length, expectedCommand, expectedResponseLength));
            }

            return result;
        }
    }
}