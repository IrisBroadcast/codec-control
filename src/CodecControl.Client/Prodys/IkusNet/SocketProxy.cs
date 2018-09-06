using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using CodecControl.Client.Exceptions;
using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;
using NLog;

namespace CodecControl.Client.Prodys.IkusNet
{
    public class SocketProxy : IDisposable
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly Socket _socket;

        public int Send(byte[] buffer)
        {
            return _socket.Send(buffer);
        }

        public int Receive(byte[] buffer)
        {
            return _socket.Receive(buffer);
        }

        public void Close()
        {
        }

        protected SocketProxy(Socket socket)
        {
            _socket = socket;
        }


        public void Dispose()
        {
            // TODO: Lämna tillbaka socket-instansen till poolen men stäng aldrig socketen
        }
    }
}