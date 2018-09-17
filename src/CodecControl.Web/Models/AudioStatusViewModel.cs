using System.Collections.Generic;
using CodecControl.Client.Models;

namespace CodecControl.Web.Models
{
    public class AudioStatusViewModel 
    {
        public VuValues VuValues { get; set; }
        public List<InputStatus> InputStatuses { get; set; }
        public List<bool> Gpos { get; set; }

    }
}