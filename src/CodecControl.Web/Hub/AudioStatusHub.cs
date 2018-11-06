using System;
using System.Threading.Tasks;
using CodecControl.Web.HostedServices;

namespace CodecControl.Web.Hub
{
    // Contains methods for the client to call
    // For methods Server->Client see AudioStatusService
    public class AudioStatusHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly AudioStatusService _audioStatusService;

        public AudioStatusHub(AudioStatusService audioStatusService)
        {
            _audioStatusService = audioStatusService;
        }

        public void Subscribe(string sipAddress)
        {
            _audioStatusService.Subscribe(Context.ConnectionId, sipAddress);
        }

        public void Unsubscribe(string sipAddress = "")
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                _audioStatusService.Unsubscribe(Context.ConnectionId);
            }
            else
            {
                _audioStatusService.Unsubscribe(Context.ConnectionId, sipAddress);
            }
        }

        public override async Task OnConnectedAsync()
        {
            // Do nothing
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _audioStatusService.Unsubscribe(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

    }
}