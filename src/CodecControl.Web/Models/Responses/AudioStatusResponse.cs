using System.Collections.Generic;
using CodecControl.Client.Models;

namespace CodecControl.Web.Models.Responses
{
    public class AudioStatusResponse 
    {
        public VuValues VuValues { get; set; }
        public List<InputStatus> InputStatus { get; set; }
        public List<bool> Gpos { get; set; }

    }
}