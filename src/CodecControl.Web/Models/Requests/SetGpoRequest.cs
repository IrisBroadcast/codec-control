namespace CodecControl.Web.Models.Requests
{
    public class SetGpoRequest
    {
        public string SipAddress { get; set; }
        public int Number { get; set; }
        public bool Active { get; set; }
    }
}