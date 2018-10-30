namespace CodecControl.Web.Models.Requests
{
    public class SetInputGainRequest
    {
        public string SipAddress { get; set; }
        public int Input { get; set; }
        public int Level { get; set; }
    }
}