#region copyright
/*
 * Copyright (c) 2018 Sveriges Radio AB, Stockholm, Sweden
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
 #endregion

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