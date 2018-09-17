using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace CodecControl.Client.Prodys.IkusNet
{

    /// <summary>
    /// Holds a dictionary with connected sockets with ip address as key.
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

        public async Task<SocketProxy> TakeSocket(string ipAddress)
        {
            using (new TimeMeasurer("Taking socket"))
            {
                //log.Info($"Taking socket for {ipAddress}");

                var dictionaryForIpAddress = _dictionary.GetOrAdd(ipAddress, s =>
                {
                    log.Info($"Creating new Bag for connections to {ipAddress}");
                    return new ConcurrentBag<ProdysSocket>();
                });

                if (dictionaryForIpAddress.TryTake(out var socket))
                {
                    log.Info($"Reusing existing socket for IP {ipAddress} found in pool. (Socket #{socket.GetHashCode()})");
                    return new SocketProxy(socket, this);
                }

                //log.Info($"Socket to IP {ipAddress} not found in pool.");
                socket = await ProdysSocket.GetConnectedSocketAsync(ipAddress);
                log.Info($"New socket to IP {ipAddress} created. (Socket #{socket.GetHashCode()})");
                return new SocketProxy(socket, this);
            }
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

            log.Info($"Returning socket for {socket.IpAddress} to pool. (Socket #{socket.GetHashCode()})");
            var dictionaryForIpAddress = _dictionary.GetOrAdd(socket.IpAddress, s => new ConcurrentBag<ProdysSocket>());
            socket.RefreshEvictionTime();
            dictionaryForIpAddress.Add(socket);
        }

        private void EvictOldSockets()
        {
            try
            {
                log.Info("Checking pool for expired sockets.");

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
                            log.Warn(
                                $"Pool entry for IP address {ipAddress} not found when trying to remove it from pool.");
                        }

                        continue;
                    }

                    var nrOfSockets = socketsBag.Count;
                    log.Info($"Found #{nrOfSockets} socket(s) for IP {ipAddress}");

                    // Remove all sockets and re-add non-exired ones.
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
                        if (!prodysSocket.IsOld())
                        {
                            //log.Info($"Re-adding socket to IP {ipAddress} to Socket Pool (Socket #{prodysSocket.GetHashCode()})");
                            socketsBag.Add(prodysSocket);
                        }
                        else
                        {
                            try
                            {
                                log.Info($"Closing socket to IP {ipAddress} because not recently used. (Socket #{prodysSocket.GetHashCode()})");
                                prodysSocket.Close();
                                prodysSocket.Dispose();
                            }
                            catch (Exception ex)
                            {
                                log.Warn(ex, "Exception when disposing ProdysSocket");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "Exception when evicting expired sockets from pool.");
            }
        }

        public void Dispose()
        {
            _evictionTimer?.Change(Timeout.Infinite, 0);
            _evictionTimer?.Dispose();
        }
    }
}