namespace CodecControl.Client.SR.BaresipRest
{
    public class BaresipLineStatus
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
        ConnectedReceived,
        Disconnecting
    }

    public class BaresipCall
    {
        public int Code { get; set; } // Status code for last call. Usually a SIP Response code, but 0 when last call terminated normally.
        public string Message { get; set; } // Status message for last call.
        public string ConnectedTo { get; set; }
    }

}