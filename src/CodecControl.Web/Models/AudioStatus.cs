﻿using System.Collections.Generic;

namespace CodecControl.Web.Models
{
    public class AudioStatus
    {
        public VuValues VuValues { get; set; }
        public List<InputStatus> InputStatuses { get; set; }
        public List<bool> Gpos { get; set; }

    }
}