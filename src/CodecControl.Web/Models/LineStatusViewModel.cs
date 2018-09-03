using CodecControl.Client.Models;
using CodecControl.Web.Controllers;
using CodecControl.Web.Helpers;

namespace CodecControl.Web.Models
{
    public class LineStatusViewModel
    {
        public string Status { get; set; }
        public string RemoteAddress { get; set; }
        public LineStatusCode LineStatus { get; set; }
        public DisconnectReason DisconnectReason { get; set; }

        public EnumDto LineStatusDto { get { return EnumDto.Create(LineStatus); } }
        public EnumDto DisconnectReasonDto { get { return EnumDto.Create(DisconnectReason); } }
        public string Error { get; set; }
    }
}