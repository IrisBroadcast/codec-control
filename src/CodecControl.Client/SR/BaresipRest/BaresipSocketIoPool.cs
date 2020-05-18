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
using CodecControl.Client.Prodys.IkusNet;
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
        private readonly ConcurrentDictionary<string, ConcurrentBag<SocketIoClient>> _dictionary;

        public BaresipSocketIoPool()
        {
            log.Debug("Baresip WebSocket pool constructor");
            _dictionary = new ConcurrentDictionary<string, ConcurrentBag<SocketIoClient>>();
        }

        //public async Task<BaresipSocketIoClient> TakeSocket(string ipAddress)
        //{

        //    using (new TimeMeasurer("Baresip Taking socket"))
        //    {
        //        var dictionaryForIpAddress = _dictionary.GetOrAdd(ipAddress, s =>
        //        {
        //            log.Info($"Baresip socket Creating new Bag for connections to {ipAddress}");
        //            return new ConcurrentBag<SocketIoClient>();
        //        });

        //        if (dictionaryForIpAddress.TryTake(out var socket))
        //        {
        //            log.Debug($"Baresip Reusing existing socket for IP {ipAddress} found in socket-pool. (Socket #{socket.GetHashCode()})");
        //            return new BaresipSocketIoClient(socket, this);
        //        }

        //        socket = await BaresipSocketIoClient.GetConnectedSocketAsync(ipAddress);
        //        log.Info($"Baresip New socket to IP {ipAddress} created. (Socket #{socket.GetHashCode()})");
        //        return new BaresipSocketIoClient(socket, this);
        //    }
        //}

        public void Dispose()
        {
            // Hmm do something here?
        }
    }
}