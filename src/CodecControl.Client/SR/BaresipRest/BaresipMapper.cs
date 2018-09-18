using System;
using CodecControl.Client.Models;

namespace CodecControl.Client.SR.BaresipRest
{
    public class BaresipMapper
    {
        public DisconnectReason MapToDisconnectReason(int statusCode)
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

        public LineStatusCode MapToLineStatusCode(BaresipState baresipState)
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
                case BaresipState.Disconnecting:
                    return LineStatusCode.Disconnecting;
                default:
                    return LineStatusCode.Disconnected;
            }
        }

        public static InputStatus MapToInputStatus(Input i)
        {
            return new InputStatus()
            {
                Enabled = i.On,
                GainLevel = i.Level
            };
        }

        public static VuValues MapToVuValues(BaresipAudioStatus bareSipAudioStatus)
        {
            return new VuValues()
            {
                RxLeft = bareSipAudioStatus.Meters.Rx.L,
                RxRight = bareSipAudioStatus.Meters.Rx.R,
                TxLeft = bareSipAudioStatus.Meters.Tx.L,
                TxRight = bareSipAudioStatus.Meters.Tx.R
            };
        }
    }
}