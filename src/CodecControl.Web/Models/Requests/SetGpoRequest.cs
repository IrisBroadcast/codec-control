namespace CodecControl.Web.Models.Requests
{
    public class SetGpoRequest :RequestBase
    {
        public int Number { get; set; }
        public bool Active { get; set; }
    }
}