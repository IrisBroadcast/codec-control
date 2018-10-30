using System.Collections.Generic;

namespace CodecControl.Web.Models.Requests
{
    public class CallRequest
    {
        public string SipAddress { get; set; }
        public string Callee { get; set; }
        public string ProfileName { get; set; }
    }


    public class HangupRequest
    {
        public string SipAddress { get; set; }
    }

    public class RebootRequest
    {
        public string SipAddress { get; set; }
    }



    public class SetGpoRequest
    {
        public string SipAddress { get; set; }
        public int Number { get; set; }
        public bool Active { get; set; }
    }

    public class SetInputEnabledRequest
    {
        public string SipAddress { get; set; }
        public int Input { get; set; }
        public bool Enabled { get; set; }
    }

    public class SetInputGainRequest
    {
        public string SipAddress { get; set; }
        public int Input { get; set; }
        public int Level { get; set; }
    }

    public class ChangeGainRequest
    {
        public string SipAddress { get; set; }
        public int Input { get; set; }
    }

    public class BatchEnableInputsRequest
    {
        public string SipAddress { get; set; }
        public IEnumerable<InputEnable> InputEnableCommands { get; set; }

        public class InputEnable
        {
            public int Input { get; set; }
            public bool Enabled { get; set; }
        }
    }
}