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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CodecControl.Client.Prodys.IkusNet;
using Microsoft.Extensions.Logging;
using NLog;
using Socket.Io.Client.Core;

namespace CodecControl.Client.SR.BaresipRest
{
   /// <summary>
    /// Holds a dictionary with connected sockets with ip address as key.
    /// </summary>
    public class BaresipSocketIoPool : IDisposable
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly ConcurrentDictionary<string, BaresipSocketIoClient> _dictionary;
        private readonly Timer _evictionTimer;

        public BaresipSocketIoPool()
        {
            log.Info("WebSocket pool constructor");
            _dictionary = new ConcurrentDictionary<string, BaresipSocketIoClient>();
            _evictionTimer = new Timer(state => { EvictUnusedSockets(); }, null,
                TimeSpan.FromSeconds(15),
                TimeSpan.FromSeconds(15));
        }

        public async Task<BaresipSocketIoClient> TakeSocket(string ipAddress)
        {
            using (new TimeMeasurer("Taking socket IO Client"))
            {
                // Check if there is a socket or create a new one
                var socketForIpAddress = _dictionary.GetOrAdd(ipAddress, s =>
                {
                    log.Info($"Creating new socket for connections to {ipAddress}");
                    return new BaresipSocketIoClient(ipAddress);
                });

                // Bump up the interest time
                socketForIpAddress.Connect();
                socketForIpAddress.RefreshEvictionTime();
                log.Debug($"Using connected socket for IP {ipAddress}. (Socket #{socketForIpAddress.GetHashCode()})");
                return socketForIpAddress;
            }
        }

        private void EvictUnusedSockets()
        {
            try
            {
                log.Debug("Checking websocket pool for expired sockets.");

                foreach (KeyValuePair<string, BaresipSocketIoClient> socketDictionary in _dictionary)
                {
                    var ipAddress = socketDictionary.Key;
                    var socket = socketDictionary.Value;

                    if (socket.IsOld())
                    {
                        // Remove empty dictionary, discard it and move on to next ip address
                        if (_dictionary.TryRemove(ipAddress, out socket))
                        {
                            log.Info($"Socket pool entry for IP address {ipAddress} is empty and was removed from pool.");
                            //socket.Close();
                            socket.Dispose();
                        }
                        else
                        {
                            log.Warn($"Socket pool entry for IP address {ipAddress} not found when trying to remove it from pool.");
                        }
                    }
                }
                log.Info($"Found #{_dictionary.Count} Websocket(s) ");
            }
            catch (Exception ex)
            {
                log.Error(ex, "Exception when evicting expired websockets from pool.");
            }
        }

        public void Dispose()
        {
            _evictionTimer?.Change(Timeout.Infinite, 0);
            _evictionTimer?.Dispose();
        }
    }
}