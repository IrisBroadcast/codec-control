#region copyright
/*
 * Copyright (c) 2018 Sveriges Radio AB, Stockholm, Sweden
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
 #endregion

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
    public class ProdysSocketPool : IDisposable
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly ConcurrentDictionary<string, ConcurrentBag<ProdysSocket>> _dictionary;
        private readonly Timer _evictionTimer;

        public ProdysSocketPool()
        {
            log.Debug("IkusNet Socket pool constructor");
            _dictionary = new ConcurrentDictionary<string, ConcurrentBag<ProdysSocket>>();
            _evictionTimer = new Timer(state => { EvictOldSockets(); }, null,
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(10));
        }

        public async Task<ProdysSocketProxy> TakeSocket(string ipAddress)
        {
            using (new TimeMeasurer("IkusNet Taking socket"))
            {
                var dictionaryForIpAddress = _dictionary.GetOrAdd(ipAddress, s =>
                {
                    log.Info($"IkusNet Creating new Bag for connections to {ipAddress}");
                    return new ConcurrentBag<ProdysSocket>();
                });

                if (dictionaryForIpAddress.TryTake(out var socket))
                {
                    log.Debug($"IkusNet Reusing existing socket for IP {ipAddress} found in pool. (Socket #{socket.GetHashCode()})");
                    return new ProdysSocketProxy(socket, this);
                }

                socket = await ProdysSocket.GetConnectedSocketAsync(ipAddress);
                log.Info($"IkusNet New socket to IP {ipAddress} created. (Socket #{socket.GetHashCode()})");
                return new ProdysSocketProxy(socket, this);
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

            log.Debug($"IkusNet Returning socket for {socket.IpAddress} to pool. (Socket #{socket.GetHashCode()})");
            var dictionaryForIpAddress = _dictionary.GetOrAdd(socket.IpAddress, s => new ConcurrentBag<ProdysSocket>());
            socket.RefreshEvictionTime();
            dictionaryForIpAddress.Add(socket);
        }

        private void EvictOldSockets()
        {
            try
            {
                log.Debug("IkusNet Checking pool for expired sockets.");

                foreach (KeyValuePair<string, ConcurrentBag<ProdysSocket>> dictionaryForIpAddress in _dictionary)
                {
                    var ipAddress = dictionaryForIpAddress.Key;
                    var socketsBag = dictionaryForIpAddress.Value;

                    if (socketsBag.IsEmpty)
                    {
                        // Remove empty dictionary, discard it and move on to next ip address
                        if (_dictionary.TryRemove(ipAddress, out socketsBag))
                        {
                            log.Info($"IkusNet Socket pool entry for IP address {ipAddress} is empty and was removed from pool.");
                        }
                        else
                        {
                            log.Warn($"IkusNet Socket pool entry for IP address {ipAddress} not found when trying to remove it from pool.");
                        }

                        continue;
                    }

                    var nrOfSockets = socketsBag.Count;
                    log.Debug($"IkusNet Found #{nrOfSockets} socket(s) for IP {ipAddress}");

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
                            log.Warn($"IkusNet Socket not found in pool. Probably other thread got it.");
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
                                // TODO: How does this acumulate?
                                log.Info($"IkusNet Closing socket to IP {ipAddress} because not recently used. (Socket #{prodysSocket.GetHashCode()})");
                                prodysSocket.Close();
                                prodysSocket.Dispose();
                            }
                            catch (Exception ex)
                            {
                                log.Warn(ex, "IkusNet Exception when disposing ProdysSocket");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "IkusNet Exception when evicting expired sockets from pool.");
            }
        }

        public void Dispose()
        {
            _evictionTimer?.Change(Timeout.Infinite, 0);
            _evictionTimer?.Dispose();
        }
    }
}