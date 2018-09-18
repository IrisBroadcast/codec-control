using CodecControl.Client.Models;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;
using CodecControl.Client.Prodys.IkusNet.Sdk.Responses;

namespace CodecControl.Client.Prodys.IkusNet
{
    public class IkusNetMapper
    {
        public static VuValues MapToVuValues(IkusNetGetVumetersResponse response)
        {
            return new VuValues
            {
                TxLeft = response.ProgramTxLeft,
                TxRight = response.ProgramTxRight,
                RxLeft = response.ProgramRxLeft,
                RxRight = response.ProgramRxRight
            };
        }

        public static DisconnectReason MapToDisconnectReason(IkusNetStreamingDisconnectionReason ikusNetStreamingDisconnectionReason)
        {
            return (DisconnectReason)ikusNetStreamingDisconnectionReason;
        }

        public static LineStatusCode MapToLineStatus(IkusNetLineStatus ikusNetLineStatus)
        {
            switch (ikusNetLineStatus)
            {
                case IkusNetLineStatus.Disconnected:
                    return LineStatusCode.Disconnected;
                case IkusNetLineStatus.Calling:
                    return LineStatusCode.Calling;
                case IkusNetLineStatus.ReceivingCall:
                    return LineStatusCode.ReceivingCall;
                case IkusNetLineStatus.ConnectedReceived:
                    return LineStatusCode.ConnectedReceived;
                case IkusNetLineStatus.ConnectedCalled:
                    return LineStatusCode.ConnectedCalled;
                case IkusNetLineStatus.Disconnecting:
                    return LineStatusCode.Disconnecting;
                default:
                    return LineStatusCode.Unknown;
            }
        }
        
        public static AudioAlgorithm MapToAudioAlgorithm(IkusNetDspAudioAlgorithm ikusNetDspAudioAlgorithm)
        {
            return (AudioAlgorithm)ikusNetDspAudioAlgorithm;
        }
    }
}