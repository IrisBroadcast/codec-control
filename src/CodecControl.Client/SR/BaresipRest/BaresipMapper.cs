using System;
using CodecControl.Client.Models;

namespace CodecControl.Client.SR.BaresipRest
{
    public class BaresipMapper
    {
        public static AudioAlgorithm MapToAudioAlgorithm(string baresipAudioAlgorithm)
        {

            if (Enum.TryParse(baresipAudioAlgorithm, true, out AudioAlgorithm audioAlgorithm))
            {
                return audioAlgorithm;
            }
            return AudioAlgorithm.Error;
        }

        public static DisconnectReason MapToDisconnectReason(int statusCode)
        {
            if (statusCode == 0)
            {
                return DisconnectReason.SipOk;
            }

            if (Enum.TryParse(statusCode.ToString(), out DisconnectReason disconnectReason))
            {
                return disconnectReason;
            }

            return DisconnectReason.None;
        }

        public static LineStatusCode MapToLineStatusCode(BaresipState baresipState)
        {
            switch (baresipState)
            {
                case BaresipState.Idle:
                    return LineStatusCode.Disconnected;
                case BaresipState.Calling:
                    return LineStatusCode.Calling;
                case BaresipState.ReceivingCall:
                    return LineStatusCode.ReceivingCall;
                case BaresipState.ConnectedReceived:
                    return LineStatusCode.ConnectedReceived;
                case BaresipState.ConnectedCalled:
                    return LineStatusCode.ConnectedCalled;
                default:
                    return LineStatusCode.Disconnected;
            }
        }

        public static InputStatus MapToInputStatus(Input i)
        {
            return new InputStatus()
            {
                Index = i.Id,
                Enabled = i.On,
                GainLevel = i.Level
            };
        }

    }
}