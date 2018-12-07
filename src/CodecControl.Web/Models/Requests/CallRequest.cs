namespace CodecControl.Web.Models.Requests
{
    public class CallRequest : RequestBase
    {
        public string Callee { get; set; }
        public string ProfileName { get; set; }
    }
}