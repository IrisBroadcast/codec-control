using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Web.CCM;
using CodecControl.Web.Models.Responses;
using Microsoft.AspNetCore.SignalR;
using NLog;

namespace CodecControl.Web.Hub
{
    public class AudioStatusUpdater
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly IHubContext<AudioStatusHub> _hub;
        private readonly CcmService _ccmService;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<SubscriptionInfo> _subscriptions = new List<SubscriptionInfo>();
        private readonly TimeSpan _pollDelay = TimeSpan.FromMilliseconds(500);
        private bool _isPolling;

        public AudioStatusUpdater(IHubContext<AudioStatusHub> hub, CcmService ccmService, IServiceProvider serviceProvider)
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

            if (_subscriptions.Any(s => s.ConnectionId == connectionId && s.SipAddress == sipAddress))
            {
                // Already subscribing
                return;
            }

            _subscriptions.Add(new SubscriptionInfo { ConnectionId = connectionId, SipAddress = sipAddress });
            _hub.Groups.AddToGroupAsync(connectionId, sipAddress);

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
            var subscriptions = _subscriptions.Where(s => s.ConnectionId == connectionId);
            _subscriptions.RemoveAll(s => s.ConnectionId == connectionId);

            foreach (var subscription in subscriptions)
            {
                _hub.Groups.RemoveFromGroupAsync(connectionId, subscription.SipAddress);
            }
        }

        private bool HasSubscriptions => _subscriptions.Any();

        public List<SubscriptionInfo> Subscriptions => _subscriptions;

        private void StartCheckCodecs()
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
                    int waitTime;
                    using (var timeMeasurer = new TimeMeasurer("Checking audio status on all codecs"))
                    {
                        var sipAddresses = _subscriptions.Select(s => s.SipAddress).Distinct().ToList();
                        log.Info($"Checking audio status on #{sipAddresses.Count} codec(s). ({string.Join(",", sipAddresses)})");

                        Parallel.ForEach(sipAddresses, async sipAddress =>
                        {
                            using (new TimeMeasurer($"Checking audio status on {sipAddress}"))
                            {
                                await CheckAudioStatusOnCodecAsync(sipAddress);
                            }
                        });
                        waitTime = (int)Math.Max(_pollDelay.Subtract(timeMeasurer.ElapsedTime).TotalMilliseconds, 0);
                    }
                    log.Debug($"Waiting {waitTime} ms until next update");
                    await Task.Delay(waitTime);
                }
                _isPolling = false;
            });

        }

        private async Task CheckAudioStatusOnCodecAsync(string sipAddress)
        {
            try
            {
                var codecInformation = await  _ccmService.GetCodecInformationBySipAddress(sipAddress);

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

                var audioStatus = await codecApi.GetAudioStatusAsync(codecInformation.Ip, codecInformation.NrOfInputs, codecInformation.NrOfGpos);

                var model = new AudioStatusResponse()
                {
                    Gpos = audioStatus.Gpos,
                    InputStatus = audioStatus.InputStatus,
                    VuValues = audioStatus.VuValues
                };

                await SendAudioStatusToClients(sipAddress, model);
            }
            catch (Exception ex)
            {
                log.Warn(ex, $"Exception when checking audio status on {sipAddress}");
            }
        }

        private async Task SendAudioStatusToClients(string sipAddress, AudioStatusResponse audioStatus)
        {
            await _hub.Clients.Group(sipAddress).SendAsync("AudioStatus", sipAddress, audioStatus);
        }

    }
}