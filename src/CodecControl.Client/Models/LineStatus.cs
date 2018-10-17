namespace CodecControl.Client.Models
{
    public class LineStatus
    {
        public LineStatusCode StatusCode { get; set; }
        public DisconnectReason DisconnectReason { get; set; }
        public string RemoteAddress { get; set; }
    }
}