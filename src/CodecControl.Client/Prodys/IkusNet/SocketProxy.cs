using System;
using NLog;

namespace CodecControl.Client.Prodys.IkusNet
{
    /// <summary>
    /// Kapslar in en ProdysSocket.
    /// Vid dispose lämnas socketen tillbaka till poolen.
    /// </summary>
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
        
        public void Dispose()
        {
            // Lämna tillbaka socket-instansen till poolen men stäng aldrig socketen
            _socketPool.ReleaseSocket(_socket);
        }
    }
}