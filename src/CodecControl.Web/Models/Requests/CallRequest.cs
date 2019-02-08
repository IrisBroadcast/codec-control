namespace CodecControl.Web.Models.Requests
{
    public class CallRequest
    {
        public string SipAddress { get; set; }
        public string Callee { get; set; }
        public string ProfileName { get; set; }
        public string WhichCodec { get; set; }
    }
}