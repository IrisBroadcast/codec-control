namespace CodecControl.Client.Models
{
    public class LineStatus
    {
        public string RemoteAddress { get; set; }
        public LineStatusCode StatusCode { get; set; }
        public DisconnectReason DisconnectReason { get; set; }
    }
}