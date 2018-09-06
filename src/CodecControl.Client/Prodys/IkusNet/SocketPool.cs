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
            log.Debug("Socket pool constructor");
            _dictionary = new ConcurrentDictionary<string, ConcurrentBag<ProdysSocket>>();
            _evictionTimer = new Timer(state => { EvictOldSockets(); }, null,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(10));
        }

        public async Task<SocketProxy> GetSocket(string ipAddress)
        {
            log.Info($"Getting socket for {ipAddress}");

            var dictionaryForIpAddress = _dictionary.GetOrAdd(ipAddress, s =>
            {
                log.Info($"Creating new Bag for connections to {ipAddress}");
                return new ConcurrentBag<ProdysSocket>();
            });

            if (!dictionaryForIpAddress.TryTake(out var socket))
            {
                log.Info($"Socket to {ipAddress} not found in socket pool. Creating new socket connection.");
                socket = await ProdysSocket.GetConnectedSocketAsync(ipAddress);
                log.Info($"Socket created. (Socket {socket.GetHashCode()})");
            }
            else
            {
                log.Info($"Reusing socket found in pool. (Socket {socket.GetHashCode()})");
            }

            return new SocketProxy(socket, this);
        }

        public void ReleaseSocket(ProdysSocket socket)
        {
            if (socket == null)
            {
                return;
            }

            if (!socket.Connected)
            {
                return;
            }

            log.Info($"Returning socket for {socket.IpAddress} to socket pool. (Socket {socket.GetHashCode()})");
            var dictionaryForIpAddress = _dictionary.GetOrAdd(socket.IpAddress, s => new ConcurrentBag<ProdysSocket>());
            socket.RefreshEvictionTime();
            dictionaryForIpAddress.Add(socket);
        }

        private void EvictOldSockets()
        {
            try
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
                    log.Info($"Found #{nrOfSockets} sockets for IP {ipAddress}");

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

                    foreach (var prodysSocket in list)
                    {
                        if (prodysSocket.IsOld())
                        {
                            try
                            {
                                log.Info($"Closing socket to IP {ipAddress} because of not recently used. (Socket {prodysSocket.GetHashCode()})");
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
                            log.Info($"Re-adding socket to IP {ipAddress} to Socket Pool (Socket {prodysSocket.GetHashCode()})");
                            socketsBag.Add(prodysSocket);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Exception when purging old sockets from pool.");
            }
        }

        public void Dispose()
        {
            _evictionTimer?.Change(Timeout.Infinite, 0);
            _evictionTimer?.Dispose();
        }
    }
}