namespace CodecControl.Web.Models.Requests
{
    public class SetInputGainRequest : RequestBase
    {
        public int Input { get; set; }
        public int Level { get; set; }
    }
}