namespace CodecControl.Web.Models.Requests
{
    public class SetInputEnabledRequest : RequestBase
    {
        public int Input { get; set; }
        public bool Enabled { get; set; }
    }
}