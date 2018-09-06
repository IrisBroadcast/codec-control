using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace CodecControl.Client.Prodys.IkusNet
{

    /// <summary>
    /// Håller dictionary med uppkopplade sockets där ip-adress är nyckel
    /// </summary>
    public class SocketPool : IDisposable
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
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

        public void ReleaseSocket(ProdysSocket socket)
        {
            var dictionaryForIpAddress = _dictionary.GetOrAdd(socket.IpAddress, s => new ConcurrentBag<ProdysSocket>());
            socket.RefreshEvictionTime();
            dictionaryForIpAddress.Add(socket);
        }

        private void EvictOldSockets()
        {
            log.Info("Checking socket pool for old sockets.");

            foreach (KeyValuePair<string, ConcurrentBag<ProdysSocket>> dictionaryForIpAddress in _dictionary)
            {
                var ipAddress = dictionaryForIpAddress.Key;
                var socketsBag = dictionaryForIpAddress.Value;

                if (socketsBag.IsEmpty)
                {
                    // Remove empty dictionary, discard it and move on to next ip address
                    if (_dictionary.TryRemove(ipAddress, out socketsBag))
                    {
                        log.Info($"Pool entry for IP address {ipAddress} is empty and was removed from pool.");
                    }
                    else
                    {
                        log.Warn($"Pool entry for IP address {ipAddress} not found when trying to remove it from pool.");
                    }
                    continue;
                }

                var nrOfSockets = socketsBag.Count;

                var list = new List<ProdysSocket>();

                while (!socketsBag.IsEmpty)
                {
                    if (socketsBag.TryTake(out ProdysSocket socket))
                    {
                        list.Add(socket);
                    }
                    else
                    {
                        log.Warn($"Socket not found in pool. Probably other thread got it.");
                    }
                }

                log.Info($"Found #{nrOfSockets} sockets for IP {ipAddress}");

                foreach (var prodysSocket in list)
                {
                    if (prodysSocket.IsOld())
                    {
                        try
                        {
                            log.Info($"Closing socket to IP {ipAddress} because of not recently used.");
                            prodysSocket.Close();
                            prodysSocket.Dispose();
                        }
                        catch (Exception ex)
                        {
                            log.Warn(ex, "Exception when disposing ProdysSocket");
                        }
                    }
                    else
                    {
                        log.Info($"Re-adding socket to IP {ipAddress} to Socket Pool");
                        socketsBag.Add(prodysSocket);
                    }
                }
            }
        }

        public void Dispose()
        {
            _evictionTimer?.Dispose();
        }
    }
}