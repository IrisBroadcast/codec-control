namespace CodecControl.Client.Prodys.IkusNet.Sdk.Enums
{
    public enum IkusNetLineStatus
    {
        NoPhysicalLine = 0,
        Disconnected,
        Disconnecting,
        Calling,
        ReceivingCall,
        ConnectedCalled,
        ConnectedReceived,
        NotAvailable,
        NegotiatingDhcp,
        Reconnecting,
        ConnectedTestingLine,
        ConnectedUploadingFile,
        ConnectedDownloadingFile,
        Initializing
    }
}