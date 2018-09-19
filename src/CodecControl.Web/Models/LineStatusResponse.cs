
namespace CodecControl.Web.Models
{
    public class LineStatusResponse
    {
        public string LineStatus { get; set; }
        public int DisconnectReasonCode { get; set; }
        public string DisconnectReasonDescription { get; set; }

    }
}