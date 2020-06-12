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
using System.Threading.Tasks;
using CodecControl.Client.SR.BaresipRest.Sdk;
using NLog;
using SocketIOClient;


namespace CodecControl.Client.SR.BaresipRest
{
    public class BaresipSocketIoClient : IDisposable
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        private static readonly TimeSpan LingerTimeSpan = TimeSpan.FromSeconds(30);
        private readonly SocketIO _socketIoClient;
        public string IpAddress { get; }

        // Sets a time in the future, if no one has refreshed interest in the device/connection
        private DateTime _evictionTime = DateTime.Now.Add(LingerTimeSpan);

        public BaresipSocketIoClient(string ipAddressStr)
        {
            IPAddress ipAddress = GetIpAddress(ipAddressStr);
            IpAddress = ipAddress.ToString();

            var conIp = "http://" + IpAddress + ":" + Baresip.ExternalProtocolIpCommandsPort;

            _socketIoClient = new SocketIO(new Uri(conIp), new SocketIOOptions
            {
                ConnectionTimeout = TimeSpan.FromSeconds(6)
            });

            try
            {
                _socketIoClient.On("system--initiation", response =>
                {
                    try
                    {
                        BaresipWsInitalData data = response.GetValue<BaresipWsInitalData>();
                        log.Debug($"Answer Mode: {data.AnswerMode}");
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                });

                _socketIoClient.OnConnected += async (sender, e) =>
                {
                    log.Info("Websocket to Baresip initiated, sending client-join");
                    await _socketIoClient.EmitAsync("client-join", "");
                };
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        class BaresipWsInitalData
        {
            public bool Redial { get; set; }
            public string SavedUri { get; set; }
            public string SavedDisplayname { get; set; }
            //public bool GuiClock { get; set; }
            //public string audiocard { get; set; }Object
            public string ScreenRotation { get; set; }
            public string AnswerMode { get; set; }
            //public string profiles: { get; set; }[],
            public string SelectedProfile { get; set; }
            public string DeviceType { get; set; }
        }

        public async void Connect()
        {
            if (_socketIoClient != null) {
                if (!_socketIoClient.Connected)
                {
                    log.Info($"SocketIO connecting {IpAddress}");
                    await _socketIoClient.ConnectAsync();
                }
                else
                {
                    log.Debug("Socket IO Client is already connected");
                }
            }
            else
            {
                log.Warn("Socket IO Client is null");
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
            log.Info($"SocketIO disposing {IpAddress}");
            await _socketIoClient.DisconnectAsync();
            Dispose(true);
        }
        #endregion

    }
}