using CodecControl.Client;

namespace CodecControl.Web.Hubs
{
    public class SubscriptionInfo
    {
        public string ConnectionId { get; set; }
        public string SipAddress { get; set; }
    }
}