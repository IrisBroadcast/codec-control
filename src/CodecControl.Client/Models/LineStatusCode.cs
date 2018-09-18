namespace CodecControl.Client.Models
{
    public enum LineStatusCode
    {
        NoPhysicalLine = 0,
        Disconnected,
        Disconnecting,
        Calling,            // Ringer upp
        ReceivingCall,
        ConnectedCalled,    // Uppkopplad. Ringde upp samtalet.
        ConnectedReceived,  // Uppkopplad. Tog emot samtalet.
        Unknown
    }
}