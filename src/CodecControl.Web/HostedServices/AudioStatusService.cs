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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Client.Exceptions;
using CodecControl.Client.Models;
using CodecControl.Client.SR.BaresipRest;
using CodecControl.Web.CCM;
using CodecControl.Web.Helpers;
using CodecControl.Web.Hub;
using CodecControl.Web.Models.Responses;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using NLog;

namespace CodecControl.Web.HostedServices
{
    public class AudioStatusService : BackgroundService
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly IHubContext<AudioStatusHub> _hub;
        private readonly CcmService _ccmService;
        private readonly IServiceProvider _serviceProvider;
        private readonly BaresipSocketIoPool _baresipSocketIoPool;
        private readonly TimeSpan _pollDelay = TimeSpan.FromMilliseconds(15500);

        public List<SubscriptionInfo> Subscriptions { get; } = new List<SubscriptionInfo>();
        private bool HasSubscriptions => Subscriptions.Any();

        public AudioStatusService(IHubContext<AudioStatusHub> hub, CcmService ccmService, IServiceProvider serviceProvider, BaresipSocketIoPool baresipSocketIoPool)
        {
            log.Debug("AudioStatusService constructor");
            _hub = hub;
            _ccmService = ccmService;
            _serviceProvider = serviceProvider;
            _baresipSocketIoPool = baresipSocketIoPool;
        }

        /// <summary>
        /// Client subscribes to information about a specific codec.
        /// </summary>
        /// <param name="connectionId">Websocket connection id</param>
        /// <param name="sipAddress">Sip address to fetch information about</param>
        /// <returns></returns>
        public async Task Subscribe(string connectionId, string sipAddress)
        {
            log.Info($"AudioStatusService Subscription from connection id {connectionId} to {sipAddress}");

            if (string.IsNullOrEmpty(sipAddress))
            {
                return;
            }

            sipAddress = sipAddress.Trim().ToLower();
            if (Subscriptions.Any(s => s.ConnectionId == connectionId && s.SipAddress == sipAddress))
            {
                // Already subscribing
                return;
            }

            var codecInformation = await _ccmService.GetCodecInformationBySipAddress(sipAddress);
            if (codecInformation == null)
            {
                log.Info($"Codec {sipAddress} is not registered in CCM");
                return;
            }

            if (string.IsNullOrEmpty(codecInformation.Ip))
            {
                log.Info($"AudioStatusService Codec {sipAddress} is not subscribable, no IP-Address found");
                return;
            }

            var codecApiType = codecInformation?.CodecApiType;
            var codecApi = codecApiType != null ? _serviceProvider.GetService(codecApiType) as ICodecApi : null;
            if (codecApi == null || string.IsNullOrEmpty(codecInformation.Ip))
            {
                log.Info($"AudioStatusService Codec {sipAddress} is not subscribable, no API found");
                return;
            }

            // Add to subscriptions list
            Subscriptions.Add(new SubscriptionInfo {
                ConnectionId = connectionId,
                SipAddress = sipAddress,
                CodecApiHasWebsocket = codecInformation?.CodecApiHasSocketConnection ?? false,
                ConnectionStarted = DateTime.UtcNow
            });

            // Add subscription to websocket group
            await _hub.Groups.AddToGroupAsync(connectionId, sipAddress);
        }

        /// <summary>
        /// Client or server removes subscription to a specific codec
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="sipAddress"></param>
        public void Unsubscribe(string connectionId, string sipAddress)
        {
            var subscriptions = Subscriptions.Where(s => s.ConnectionId == connectionId && s.SipAddress == sipAddress).ToList();
            Subscriptions.RemoveAll(s => s.ConnectionId == connectionId && s.SipAddress == sipAddress);
            if (subscriptions.Any())
            {
                _hub.Groups.RemoveFromGroupAsync(connectionId, subscriptions[0].SipAddress);
            }
        }

        /// <summary>
        /// Client or server removes all subscriptions related to a websocket connection id
        /// </summary>
        /// <param name="connectionId"></param>
        public void Unsubscribe(string connectionId)
        {
            var subscriptions = Subscriptions.Where(s => s.ConnectionId == connectionId);
            Subscriptions.RemoveAll(s => s.ConnectionId == connectionId);
            foreach (var subscription in subscriptions)
            {
                _hub.Groups.RemoveFromGroupAsync(connectionId, subscription.SipAddress);
            }
        }

        /// <summary>
        /// Should be used to keep track of current subscriptions
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            log.Info($"Queued Audio Status Service is starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                // Audio status
                while (HasSubscriptions && !stoppingToken.IsCancellationRequested)
                {
                    int waitTime;
                    using (var timeMeasurer = new TimeMeasurer("AudioStatusService checking on all codecs"))
                    {
                        //var sipAddresses = Subscriptions.Select(s => s.SipAddress).Distinct().ToList();
                        var subs = Subscriptions.Distinct().ToList();
                        log.Debug($"AudioStatusService Checking audio status on #{subs.Count} codec(s).");

                        Parallel.ForEach(subs, async sub =>
                        {
                            using (new TimeMeasurer($"AudioStatusService Checking for {sub.SipAddress}"))
                            {
                                //if (sub.CodecApiHasWebsocket)
                                //{
                                //    await StartWebsocketConnectionToCodec(sub.SipAddress);
                                //}
                                //else
                                //{
                                    await CheckAudioStatusOnCodecAsync(sub.SipAddress);
                                //}
                            }
                        });

                        waitTime = (int)Math.Max(_pollDelay.Subtract(timeMeasurer.ElapsedTime).TotalMilliseconds, 0);
                    }
                    log.Trace($"AudioStatusService Waiting {waitTime} ms until next update");
                    await Task.Delay(waitTime);
                }

                if (!stoppingToken.IsCancellationRequested)
                {
                    log.Trace("AudioStatusService / Linestatus has no subscriptions");
                    await Task.Delay(1000); // Wait until next check for HasSubscriptions
                }
            }

            log.Info("AudioStatusService finished");
        }

        private async Task StartWebsocketConnectionToCodec(string sipAddress)
        {
            try
            {
                // Get codec template data from CCM
                var codecInformation = await _ccmService.GetCodecInformationBySipAddress(sipAddress); // TODO: Not every time???? 
                if (codecInformation == null)
                {
                    log.Info($"Codec {sipAddress} is not currently registered in CCM, could't get codec information");
                    return;
                }

                if (string.IsNullOrEmpty(codecInformation.Ip))
                {
                    log.Info($"Could not start socket {sipAddress} is not subscribable");
                    return;
                }
                var socket = _baresipSocketIoPool.TakeSocket(codecInformation.Ip);
            }
            catch (UnableToConnectException ex)
            {
                log.Warn($"AudioStatusService Exception unable to connect to socket {sipAddress}.");
                log.Trace(ex, "AudioStatusService Exception");
            }
            catch (CodecInvocationException ex)
            {
                log.Warn($"AudioStatusService Failed to check audio status on socket {sipAddress}. {ex.Message}");
            }
            catch (Exception ex)
            {
                log.Warn(ex, $"AudioStatusService Exception when checking audio status on socket {sipAddress}");
            }
        }

        private async Task CheckAudioStatusOnCodecAsync(string sipAddress)
        {
            try
            {
                // Get codec template data from CCM
                var codecInformation = await _ccmService.GetCodecInformationBySipAddress(sipAddress); // TODO: Not every time???? 
                if (codecInformation == null)
                {
                    log.Info($"Codec {sipAddress} is not currently registered in CCM, could't get codec information");
                    return;
                }

                var codecApiType = codecInformation?.CodecApiType;
                var codecApi = codecApiType != null ? _serviceProvider.GetService(codecApiType) as ICodecApi : null;

                if (codecApi == null || string.IsNullOrEmpty(codecInformation.Ip))
                {
                    log.Info($"Missing information for connecting to codec {sipAddress}");
                    return;
                }

                var audioStatus = await codecApi.GetAudioStatusAsync(codecInformation.Ip, codecInformation.NrOfInputs, codecInformation.NrOfGpos);

                var model = new AudioStatusResponse()
                {
                    Gpos = audioStatus.Gpos,
                    InputStatus = audioStatus.InputStatus,
                    VuValues = audioStatus.VuValues
                };

                await SendAudioStatusToClients(sipAddress, model);
            }
            catch (UnableToConnectException ex)
            {
                log.Warn($"AudioStatusService Exception unable to connect to {sipAddress}.");
                log.Trace(ex, "AudioStatusService Exception");
            }
            catch (CodecInvocationException ex)
            {
                log.Warn($"AudioStatusService Failed to check audio status on {sipAddress}. {ex.Message}");
            }
            catch (Exception ex)
            {
                log.Warn(ex, $"AudioStatusService Exception when checking audio status on {sipAddress}");
            }
        }

        private async Task CheckLineStatusOnCodecAsync(string sipAddress)
        {
            try
            {
                // Get codec template data from CCM
                // TODO: move this out (GetCodecInformationBySipAddress), it's redundant and being used in multiple locations
                var codecInformation = await _ccmService.GetCodecInformationBySipAddress(sipAddress);
                if (codecInformation == null)
                {
                    log.Info($"Codec {sipAddress} is not currently registered in CCM, could't get codec information");
                    return;
                }

                var codecApiType = codecInformation?.CodecApiType;
                var codecApi = codecApiType != null ? _serviceProvider.GetService(codecApiType) as ICodecApi : null;

                if (codecApi == null || string.IsNullOrEmpty(codecInformation.Ip))
                {
                    log.Info($"Missing information for connecting to codec {sipAddress}");
                    return;
                }

                var line = new LineStatusResponse();

                var lineStatus = await codecApi.GetLineStatusAsync(codecInformation.Ip, "Line1"); // TODO: Create a GetLineStatuses, for multiple lines

                line.LineEncoder = string.IsNullOrEmpty(lineStatus.LineEncoder) ? "Line1" : lineStatus.LineEncoder;
                line.LineStatus = lineStatus.StatusCode.ToString();
                line.DisconnectReasonCode = (int)lineStatus.DisconnectReason;
                line.DisconnectReasonDescription = lineStatus.DisconnectReason.Description();
                line.RemoteAddress = lineStatus.RemoteAddress;

                var model = new UnitStateResponse()
                {
                    IsOnline = true
                };

                model.LineStatuses.Add(line);

                // TODO: Get actual connected to information form ccm. this is a very inefficient way, but deal with the rare condition of codec in call but not in ccm. 

                await SendLineStatusToClients(sipAddress, model);
            }
            catch (CodecInvocationException ex)
            {
                log.Info($"Failed to check line status on {sipAddress}. {ex.Message}");
            }
            catch (Exception ex)
            {
                log.Warn(ex, $"Exception when checking line status on {sipAddress}");
            }
        }

        private async Task SendAudioStatusToClients(string sipAddress, AudioStatusResponse audioStatus)
        {
            await _hub.Clients.Group(sipAddress).SendAsync("AudioStatus", sipAddress, audioStatus);
        }

        private async Task SendLineStatusToClients(string sipAddress, UnitStateResponse unitStatus)
        {
            await _hub.Clients.Group(sipAddress).SendAsync("LineStatus", sipAddress, unitStatus);
        }
    }
}