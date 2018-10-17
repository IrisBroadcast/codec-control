
namespace CodecControl.Web.Models.Responses
{
    public class LineStatusResponse
    {
        public string LineStatus { get; set; }
        public int DisconnectReasonCode { get; set; }
        public string DisconnectReasonDescription { get; set; }
        public string RemoteAddress { get; set; }
    }
}