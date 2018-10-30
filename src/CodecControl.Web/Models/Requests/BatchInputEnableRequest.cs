using System.Collections.Generic;

namespace CodecControl.Web.Models.Requests
{
    public class BatchInputEnableRequest
    {
        public string SipAddress { get; set; }
        public List<InputEnableRequest> InputEnableRequests { get; set; }

        public class InputEnableRequest
        {
            public int Input { get; set; }
            public bool Enabled { get; set; }
        }
    }
}