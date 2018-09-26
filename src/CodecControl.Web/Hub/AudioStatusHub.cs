using System;
using System.Threading.Tasks;

namespace CodecControl.Web.Hub
{
    // Contains methods for the client to call
    // For methods Server->Client see AudioStatusUpdater
    public class AudioStatusHub : Microsoft.AspNetCore.SignalR.Hub
    {
        private readonly AudioStatusUpdater _audioStatusUpdater;

        public AudioStatusHub(AudioStatusUpdater audioStatusUpdater)
        {
            _audioStatusUpdater = audioStatusUpdater;
        }

        public void Subscribe(string sipAddress)
        {
            _audioStatusUpdater.Subscribe(Context.ConnectionId, sipAddress);
        }

        public void Unsubscribe(string sipAddress = "")
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                _audioStatusUpdater.Unsubscribe(Context.ConnectionId);
            }
            else
            {
                _audioStatusUpdater.Unsubscribe(Context.ConnectionId, sipAddress);
            }
        }

        public override async Task OnConnectedAsync()
        {
            // Do nothing
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _audioStatusUpdater.Unsubscribe(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

    }
}