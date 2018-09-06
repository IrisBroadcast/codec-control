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
        private readonly ProdysSocket _socket;
        private readonly SocketPool _socketPool;
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();


        public SocketProxy(ProdysSocket socket, SocketPool socketPool)
        {
            _socket = socket;
            _socketPool = socketPool;
        }

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


        public void Dispose()
        {
            // TODO: Lämna tillbaka socket-instansen till poolen men stäng aldrig socketen
            _socketPool.AddSocket(_socket);
        }
    }
}