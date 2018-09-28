namespace CodecControl.Client.SR.BaresipRest
{
    public class BaresipResponse
    {
        public bool Success { get; set; }
    }

    public class InputGainResponse : BaresipResponse
    {
        public int Input { get; set; }
        public int Value { get; set; }
    }

    public class InputEnableResponse : BaresipResponse
    {
        public int Input { get; set; }
        public bool Value { get; set; }
    }
    
    public class IsAvailableResponse : BaresipResponse
    {
        public string Value { get; set; }
    }
    
    public class BaresipLineStatus : BaresipResponse
    {
        public BaresipState State { get; set; }
        public BaresipCall Call { get; set; }
    }

    public enum BaresipState
    {
        Idle,
        Calling,
        ReceivingCall,
        ConnectedCalled,
        ConnectedReceived
    }

    public class BaresipCall
    {
        public int Code { get; set; } // Status code for last call. Usually a SIP Response code, but 0 when last call terminated normally.
        public string Message { get; set; } // Status message for last call.
    }

}