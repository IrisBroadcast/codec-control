using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Client.Models;
using CodecControl.Web.Interfaces;
using Microsoft.AspNetCore.SignalR;
using NLog;

namespace CodecControl.Web.Hubs
{
    public class AudioStatusUpdater
    {

        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly IHubContext<AudioStatusHub> _hub;
        private readonly ICcmService _ccmService;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<SubscriptionInfo> _subscriptions = new List<SubscriptionInfo>();
        private readonly TimeSpan _pollDelay = TimeSpan.FromMilliseconds(500);
        private bool _isPolling;

        public AudioStatusUpdater(IHubContext<AudioStatusHub> hub, ICcmService ccmService, IServiceProvider serviceProvider)
        {
            log.Debug("AudioStatusUpdater constructor");
            _hub = hub;
            _ccmService = ccmService;
            _serviceProvider = serviceProvider;
        }

        public void Subscribe(string connectionId, string sipAddress)
        {
            log.Info($"Subscription from connection id {connectionId} to {sipAddress}");

            if (string.IsNullOrEmpty(sipAddress))
            {
                return;
            }

            sipAddress = sipAddress.Trim().ToLower();
            if (!_subscriptions.Any(s => s.ConnectionId == connectionId && s.SipAddress == sipAddress))
            {
                _subscriptions.Add(new SubscriptionInfo() { ConnectionId = connectionId, SipAddress = sipAddress });
                _hub.Groups.AddToGroupAsync(connectionId, sipAddress);
            }
            StartCheckCodecs();
        }

        public void Unsubscribe(string connectionId, string sipAddress)
        {
            var subscriptions = _subscriptions.Where(s => s.ConnectionId == connectionId && s.SipAddress == sipAddress).ToList();
            _subscriptions.RemoveAll(s => s.ConnectionId == connectionId && s.SipAddress == sipAddress);
            if (subscriptions.Any())
            {
                _hub.Groups.RemoveFromGroupAsync(connectionId, subscriptions[0].SipAddress);
            }
        }

        public void Unsubscribe(string connectionId)
        {
            var subscription = _subscriptions.FirstOrDefault(s => s.ConnectionId == connectionId);
            if (subscription != null)
            {
                _subscriptions.RemoveAll(s => s.ConnectionId == connectionId); // Can be more than one.
                _hub.Groups.RemoveFromGroupAsync(connectionId, subscription.SipAddress);
            }
        }

        private bool HasSubscriptions => _subscriptions.Any();

        public void StartCheckCodecs()
        {
            if (_isPolling || !HasSubscriptions)
            {
                return;
            }

            Task.Run(async () =>
           {
               _isPolling = true;
               while (HasSubscriptions)
               {
                   try
                   {
                       var sipAddresses = _subscriptions.Select(s => s.SipAddress).Distinct().ToList();
                       log.Info($"Pooling #{sipAddresses.Count} codec(s)");
                       foreach (var sipAddress in sipAddresses)
                       {
                           await CheckAudioStatusOnCodecAsync(sipAddress);
                       }
                       await Task.Delay(_pollDelay);

                   }
                   catch (Exception ex)
                   {
                       log.Warn(ex, "Exception when retrieving audio status");
                   }
               }

               _isPolling = false;
           });

        }

        private async Task CheckAudioStatusOnCodecAsync(string sipAddress)
        {
            var codecInformation = _ccmService.GetCodecInformationBySipAddress(sipAddress);

            if (codecInformation == null)
            {
                log.Info($"Codec {sipAddress} is not currently registered in CCM.");
                return;
            }

            var codecApiType = codecInformation?.CodecApiType;
            var codecApi = codecApiType != null ? _serviceProvider.GetService(codecApiType) as ICodecApi : null;

            if (codecApi == null || string.IsNullOrEmpty(codecInformation.Ip))
            {
                log.Info($"Missing information to connect to codec {sipAddress}");
                return;
            }

            var nrOfGpos = 2; // TODO: Don't hard code this. Add CCM property.
            var audioStatus = await codecApi.GetAudioStatusAsync(codecInformation.Ip, codecInformation.NrOfInputs, nrOfGpos);
            await SendAudioStatus(sipAddress, audioStatus);

        }

        private async Task SendAudioStatus(string sipAddress, AudioStatus audioStatus)
        {
            await _hub.Clients.Group(sipAddress).SendAsync("AudioStatus", audioStatus);
        }

    }
}