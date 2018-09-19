using System.Collections.Generic;

namespace CodecControl.Client.Models
{
    public class AudioStatus
    {
        public VuValues VuValues { get; set; }
        public List<InputStatus> InputStatus { get; set; }
        public List<bool> Gpos { get; set; }

    }
}