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

using System.Collections.Generic;
using System.Threading.Tasks;
using CodecControl.Client.Models;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands;
using CodecControl.Client.Prodys.IkusNet.Sdk.Responses;

namespace CodecControl.Client.Prodys.IkusNet
{
    /// <summary>
    /// This API is intended for Quantum with controllable inputs.
    /// </summary>
    public class IkusNetApi : IkusNetStApi
    {
        public IkusNetApi(SocketPool socketPool) : base(socketPool)
        {
        }

        public override async Task<bool> GetInputEnabledAsync(string hostAddress, int input)
        {
            // Works only on Quantum codec, not Quantum ST
            using (var socket = await SocketPool.TakeSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetInputEnabled { Input = input });
                var response = new IkusNetGetInputEnabledResponse(socket);
                return response.Enabled;
            }
        }

        public override async Task<int> GetInputGainLevelAsync(string hostAddress, int input)
        {
            // Works only on Quantum codec, not Quantum ST
            using (var socket = await SocketPool.TakeSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetInputGainLevel { Input = input });
                var response = new IkusNetGetInputGainLevelResponse(socket);
                return response.GainLeveldB;
            }
        }

        public override async Task<(bool, int)> GetInputGainAndStatusAsync(string hostAddress, int input)
        {
            // Works only on Quantum codec, not Quantum ST
            using (var socket = await SocketPool.TakeSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetInputEnabled { Input = input });
                var inputEnabledResponse = new IkusNetGetInputEnabledResponse(socket);
                var enabled = inputEnabledResponse.Enabled;

                SendCommand(socket, new CommandIkusNetGetInputGainLevel { Input = input });
                var gainLevelResponse = new IkusNetGetInputGainLevelResponse(socket);
                var gain = gainLevelResponse.GainLeveldB;
                return (enabled, gain);
            }

        }

        public override async Task<AudioStatus> GetAudioStatusAsync(string hostAddress, int nrOfInputs, int nrOfGpos)
        {
            var audioStatus = new AudioStatus();

            using (var socket = await SocketPool.TakeSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetVuMeters());
                var vuResponse = new IkusNetGetVumetersResponse(socket);

                audioStatus.VuValues = new VuValues
                {
                    TxLeft = vuResponse.ProgramTxLeft,
                    TxRight = vuResponse.ProgramTxRight,
                    RxLeft = vuResponse.ProgramRxLeft,
                    RxRight = vuResponse.ProgramRxRight
                };

                audioStatus.InputStatus = new List<InputStatus>();

                // Works only on Quantum codec, not Quantum ST
                for (int input = 0; input < nrOfInputs; input++)
                {
                    SendCommand(socket, new CommandIkusNetGetInputEnabled {Input = input});
                    var enabledResponse = new IkusNetGetInputEnabledResponse(socket);
                    var inputEnabled = enabledResponse.Enabled;

                    SendCommand(socket, new CommandIkusNetGetInputGainLevel {Input = input});
                    var gainLevelResponse = new IkusNetGetInputGainLevelResponse(socket);
                    var gainLevel = gainLevelResponse.GainLeveldB;

                    audioStatus.InputStatus.Add(new InputStatus
                        {Index = input, Enabled = inputEnabled, GainLevel = gainLevel});
                }

                audioStatus.Gpos = new List<GpoStatus>();

                for (int gpo = 0; gpo < nrOfGpos; gpo++)
                {
                    SendCommand(socket, new CommandIkusNetGetGpo {Gpio = gpo});
                    var response = new IkusNetGetGpoResponse(socket);
                    var gpoEnable = response.Active;
                    if (!gpoEnable.HasValue)
                    {
                        // Indication of missing GPO for the number. Probably we passed the last one.
                        break;
                    }

                    audioStatus.Gpos.Add(new GpoStatus() {Index = gpo, Active = gpoEnable.Value});
                }

                return audioStatus;
            }
        }


        public override async Task<bool> SetInputEnabledAsync(string hostAddress, int input, bool enabled)
        {
            var cmd = new CommandIkusNetSetInputEnabled { Input = input, Enabled = enabled };
            await SendConfigurationCommandAsync(hostAddress, cmd);

            var isEnabled = await GetInputEnabledAsync(hostAddress, input);
            return isEnabled;
        }

        public override async Task<int> SetInputGainLevelAsync(string hostAddress, int input, int gainLevel)
        {
            var cmd = new CommandIkusNetSetInputGainLevel { GainLeveldB = gainLevel, Input = input };
            await SendConfigurationCommandAsync(hostAddress, cmd);

            var readGainLevel = await GetInputGainLevelAsync(hostAddress, input);
            return readGainLevel;
        }

    }
}