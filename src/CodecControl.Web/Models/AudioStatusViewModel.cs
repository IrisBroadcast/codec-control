using System.Collections.Generic;
using CodecControl.Web.Controllers;

namespace CodecControl.Web.Models
{
    public class AudioStatusViewModel 
    {
        public VuValues VuValues { get; set; }
        public List<InputStatus> InputStatuses { get; set; }
        public List<bool> Gpos { get; set; }
        public string Error { get; set; }

    }
}