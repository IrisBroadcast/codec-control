namespace CodecControl.Web.Models.Requests
{
    public class SetInputEnabledRequest
    {
        public string SipAddress { get; set; }
        public int Input { get; set; }
        public bool Enabled { get; set; }
    }
}