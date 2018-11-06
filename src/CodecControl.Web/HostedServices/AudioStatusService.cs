using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Web.CCM;
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
        private readonly TimeSpan _pollDelay = TimeSpan.FromMilliseconds(500);

        public List<SubscriptionInfo> Subscriptions { get; } = new List<SubscriptionInfo>();
        private bool HasSubscriptions => Subscriptions.Any();

        public AudioStatusService(IHubContext<AudioStatusHub> hub, CcmService ccmService, IServiceProvider serviceProvider)
        {
            log.Debug("AudioStatusService constructor");
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

            if (Subscriptions.Any(s => s.ConnectionId == connectionId && s.SipAddress == sipAddress))
            {
                // Already subscribing
                return;
            }

            Subscriptions.Add(new SubscriptionInfo { ConnectionId = connectionId, SipAddress = sipAddress });
            _hub.Groups.AddToGroupAsync(connectionId, sipAddress);
        }

        public void Unsubscribe(string connectionId, string sipAddress)
        {
            var subscriptions = Subscriptions.Where(s => s.ConnectionId == connectionId && s.SipAddress == sipAddress).ToList();
            Subscriptions.RemoveAll(s => s.ConnectionId == connectionId && s.SipAddress == sipAddress);
            if (subscriptions.Any())
            {
                _hub.Groups.RemoveFromGroupAsync(connectionId, subscriptions[0].SipAddress);
            }
        }

        public void Unsubscribe(string connectionId)
        {
            var subscriptions = Subscriptions.Where(s => s.ConnectionId == connectionId);
            Subscriptions.RemoveAll(s => s.ConnectionId == connectionId);

            foreach (var subscription in subscriptions)
            {
                _hub.Groups.RemoveFromGroupAsync(connectionId, subscription.SipAddress);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            log.Debug($"Audio Status Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                while (HasSubscriptions && !stoppingToken.IsCancellationRequested)
                {
                    int waitTime;
                    using (var timeMeasurer = new TimeMeasurer("Checking audio status on all codecs"))
                    {
                        var sipAddresses = Subscriptions.Select(s => s.SipAddress).Distinct().ToList();
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

                if (!stoppingToken.IsCancellationRequested)
                {
                    log.Debug("AudioStatusService has no subscriptions");
                    await Task.Delay(1000); // Wait until next check for HasSubscriptions
                }
            }

            log.Debug("AudioStatusService finished");
        }


        private async Task CheckAudioStatusOnCodecAsync(string sipAddress)
        {
            try
            {
                var codecInformation = await _ccmService.GetCodecInformationBySipAddress(sipAddress);

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