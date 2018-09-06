using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CodecControl.Client.Prodys.IkusNet
{

    /// <summary>
    /// Håller dictionary med uppkopplade sockets där ip-adress är nyckel
    /// </summary>
    public class SocketPool
    {
        private readonly ConcurrentDictionary<string, ConcurrentBag<ProdysSocket>> _dictionary;
        private readonly Timer _evictionTimer;

        public SocketPool()
        {
            _dictionary = new ConcurrentDictionary<string, ConcurrentBag<ProdysSocket>>();
            _evictionTimer = new Timer(state => { EvictOldSockets(); }, null,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(10));
        }

        public async Task<SocketProxy> GetSocket(string ipAddress)
        {
            var dictionaryForIpAddress = _dictionary.GetOrAdd(ipAddress, s => new ConcurrentBag<ProdysSocket>());

            if (!dictionaryForIpAddress.TryTake(out var socket))
            {
                socket = await ProdysSocket.GetConnectedSocketAsync(ipAddress);
            }

            return new SocketProxy(socket, this);
        }

        public void AddSocket(ProdysSocket socket)
        {
            var dictionaryForIpAddress = _dictionary.GetOrAdd(socket.IpAddress, s => new ConcurrentBag<ProdysSocket>());
            socket.UpdateEvictionTime();
            dictionaryForIpAddress.Add(socket);
        }

        private void EvictOldSockets()
        {
            foreach (var dictionaryForIpAddress in _dictionary.Values)
            {
                var list = new List<ProdysSocket>();

                while (!dictionaryForIpAddress.IsEmpty)
                {
                    if (dictionaryForIpAddress.TryTake(out ProdysSocket socket))
                    {
                        list.Add(socket);
                    }
                }

                foreach (var prodysSocket in list)
                {
                    if (prodysSocket.IsOld())
                    {
                        try
                        {
                            prodysSocket.Close();
                            prodysSocket.Dispose();
                        }
                        catch (Exception ex)
                        {
                            // TODO: Log warning
                        }
                    }
                    else
                    {
                        dictionaryForIpAddress.Add(prodysSocket);
                    }
                }
            }
        }

    }
}