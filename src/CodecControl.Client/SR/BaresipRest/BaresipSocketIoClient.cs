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
using System.Collections.Generic;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CodecControl.Client.Exceptions;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NLog;
using Socket.Io.Client.Core;
using Socket.Io.Client.Core.Model;
using Socket.Io.Client.Core.Model.SocketEvent;
using Socket.Io.Client.Core.Model.SocketIo;
using Utf8Json;
using LogLevel = Websocket.Client.Logging.LogLevel;

namespace CodecControl.Client.SR.BaresipRest
{
    public class BaresipSocketIoClient : IDisposable
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        private static readonly TimeSpan LingerTimeSpan = TimeSpan.FromSeconds(30);
        private readonly SocketIoClient _socketIoClient;
        public string IpAddress { get; }

        // Sets a time in the future, if no one has refreshed interest in the device/connection
        private DateTime _evictionTime = DateTime.Now.Add(LingerTimeSpan);

        public BaresipSocketIoClient(string ipAddressStr)
        {
            IPAddress ipAddress = GetIpAddress(ipAddressStr);
            IpAddress = ipAddress.ToString();

            var options = new SocketIoClientOptions();
            _socketIoClient = new SocketIoClient(options);
        }

        public async void Connect()
        {
            if (_socketIoClient != null) { 
                if (!_socketIoClient.IsRunning)
                {
                    log.Info("######### CONNECTING LOAL");

                    IDisposable someEventSubscription = _socketIoClient.On("app--config")
                        .Subscribe(message =>
                        {
                            Console.WriteLine($"Received event: {message.EventName}. Data: {message.FirstData}");
                            log.Info($"Rceived {message.FirstData.ToString()}");
                        });

                    _socketIoClient.On("codec--update")
                        .Subscribe(message =>
                        {
                            Console.WriteLine($"Received event codec--update: {message.EventName}. Data: {message.FirstData}");
                            log.Info("OOK");
                            log.Info($"Received codec--update {message.FirstData.ToString()}");
                        });

                    _socketIoClient.On("system--initiation")
                        .Subscribe(message =>
                        {
                            Console.WriteLine($"Received event system--initiation: {message.EventName}. Data: {message.FirstData}");
                            log.Info("OOK");
                            log.Info($"Received system--initiation {message.FirstData.ToString()}");
                        });

                    _socketIoClient.Events.OnPacket.Subscribe(data =>
                    {
                        log.Info("OnPacket");
                        log.Info(data.ToString());
                        ProcessAckAndEvent(data);
                    });

                    _socketIoClient.Events.OnOpen.Subscribe(observer =>
                    {
                        log.Info("OnOpen");
                        _socketIoClient.Emit("client-join", "");
                    });

                    await _socketIoClient.OpenAsync(new Uri("http://localhost:8080"));


                    _socketIoClient.Emit("client-join", "");

                    //optionally unsubscribe (equivalent to off() from socket.io)
                    //someEventSubscription.Dispose();
                }
            }
            else
            {
                log.Warn("Socket IO Client is null");
            }
        }

        private void ProcessAckAndEvent(Packet packet)
        {
            try
            {
                if (packet.SocketIoType == SocketIoType.Event || packet.SocketIoType == SocketIoType.Ack)
                {
                    log.Info(packet.Id.ToString());
                    log.Info(packet.Data.ToString());
                    var eventArray = _socketIoClient.Options.JsonSerializer.Deserialize<string[]>(packet.Data);
                    if (eventArray != null && eventArray.Length > 0)
                    {
                        if (packet.Id.HasValue)
                            log.Info($"Received packet with ACK: {packet.Id.Value}");

                        if (packet.SocketIoType == SocketIoType.Ack && packet.Id.HasValue)
                        {
                            log.Info("OK");
                        }
                        else
                        {
                            //first element should contain event name
                            //we can have zero, one or multiple arguments after event name so emit based on number of them
                            var message = eventArray.Length == 1
                                ? new EventMessageEvent(eventArray[0], new List<string>())
                                : new EventMessageEvent(eventArray[0], eventArray[1..]);

                            log.Info($"Message {message}");
                        }
                    }
                }
            }
            catch (JsonParsingException ex)
            {
                log.Info(ex, $"Error while deserializing event message. Packet: {packet}");
                log.Error(ex);
                log.Error(ex.Message);
            }
        }

        public void RefreshEvictionTime()
        {
            _evictionTime = DateTime.Now.Add(LingerTimeSpan);
        }

        public bool IsOld()
        {
            return DateTime.Now > _evictionTime;
        }

        internal static Task<BaresipSocketIoClient> GetConnectedSocketAsync(string ipAddress)
        {
            throw new NotImplementedException();
        }

        private static IPAddress GetIpAddress(string address)
        {
            if (IPAddress.TryParse(address, out var ipAddress))
            {
                return ipAddress;
            }

            var ips = Dns.GetHostAddresses(address);
            return ips.Length > 0 ? ips[0] : null;
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            // Hmmpf....
        }

        public async void Dispose()
        {
            log.Info("DISPOSING");
            await _socketIoClient.CloseAsync();
            Dispose(true);
        }
        #endregion

    }
}