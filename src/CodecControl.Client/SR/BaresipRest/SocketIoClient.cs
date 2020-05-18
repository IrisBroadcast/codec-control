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
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using CodecControl.Client.Exceptions;
using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;
using NLog;

namespace CodecControl.Client.Prodys.IkusNet
{
    public class SocketIoClient : IDisposable
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static readonly TimeSpan LingerTimeSpan = TimeSpan.FromSeconds(30);

        public string IpAddress { get; }
        private DateTime _evictionTime = DateTime.Now.Add(LingerTimeSpan);

        public SocketIoClient(IPAddress ipAddress)
        {
            IpAddress = ipAddress.ToString();
        }
        
        public void RefreshEvictionTime()
        {
            _evictionTime = DateTime.Now.Add(LingerTimeSpan);
        }

        public bool IsOld()
        {
            return DateTime.Now > _evictionTime;
        }

        public static async Task<SocketIoClient> GetConnectedSocketAsync(string address, int sendTimeout = 300)
        {
            using (new TimeMeasurer("SocketIoClient.GetConnectedSocketAsync"))
            {
                IPAddress ipAddress = GetIpAddress(address);

                if (ipAddress == null)
                {
                    log.Warn($"Unable to resolve ip address for {address}");
                    throw new UnableToResolveAddressException();
                }

                SocketIoClient connectedSocket = await ConnectAsync(ipAddress, new CsConnect2(), sendTimeout);

                if (connectedSocket != null)
                {
                    return connectedSocket;
                }

                log.Warn("Unable to connect to codec at {0} using authenticated connect.", ipAddress);

                // Otherwise, try non authenticated connect
                connectedSocket = await ConnectAsync(ipAddress, new CsConnect(), sendTimeout);

                if (connectedSocket != null)
                {
                    return connectedSocket;
                }

                log.Warn("Unable to connect to codec at {0}. Both authenticated and unauthenticated connect failed.", ipAddress);
                throw new UnableToConnectException(); 
            }
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

        private static async Task<SocketIoClient> ConnectAsync(IPAddress ipAddress, ConnectCommandBase connectCmd, int sendTimeout)
        {
            SocketIoClient socket = null;

            try
            {
                socket = new SocketIoClient(ipAddress);

                if (sendTimeout > 0)
                {
                    socket.SendTimeout = sendTimeout;
                }

                var endpoint = new IPEndPoint(ipAddress, Sdk.IkusNet.ExternalProtocolIpCommandsPort);
                
                await socket.ConnectAsync(endpoint, 4000); // TODO: Added a timeout here, verify that it's ok

                return socket;
            }
            catch (Exception ex)
            {
                log.Warn(ex, $"Exception when connecting to codec at {ipAddress}, {ex.GetType().FullName}");
                log.Debug(ex, "Exception when using Prodys socket");
                socket?.Close();
                return null;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SocketIoClient()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}