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
    public class IkusNetApi : IkusNetBaseApi
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

                audioStatus.InputStatuses = new List<InputStatus>();

                // Works only on Quantum codec, not Quantum ST
                for (int input = 0; input < nrOfInputs; input++)
                {
                    SendCommand(socket, new CommandIkusNetGetInputEnabled { Input = input });
                    var enabledResponse = new IkusNetGetInputEnabledResponse(socket);
                    var inputEnabled = enabledResponse.Enabled;

                    SendCommand(socket, new CommandIkusNetGetInputGainLevel { Input = input });
                    var gainLevelResponse = new IkusNetGetInputGainLevelResponse(socket);
                    var gainLevel = gainLevelResponse.GainLeveldB;

                    audioStatus.InputStatuses.Add(new InputStatus { Enabled = inputEnabled, GainLevel = gainLevel });
                }

                //audioStatus.Gpis = new List<bool>();

                //for (int gpi = 0; gpi < nrOfGpis; gpi++)
                //{
                //    SendCommand(socket, new CommandIkusNetGetGpi { Gpio = gpi });
                //    var response = new IkusNetGetGpiResponse(socket);
                //    var gpiEnabled = response.Active;
                //    if (!gpiEnabled.HasValue)
                //    {
                //        // Indication of missing GPI for the number. Probably we passed the last one.
                //        break;
                //    }
                //    audioStatus.Gpis.Add(gpiEnabled.Value);
                //}

                audioStatus.Gpos = new List<bool>();

                for (int gpo = 0; gpo < nrOfGpos; gpo++)
                {
                    SendCommand(socket, new CommandIkusNetGetGpo { Gpio = gpo });
                    var response = new IkusNetGetGpoResponse(socket);
                    var gpoEnable = response.Active;
                    if (!gpoEnable.HasValue)
                    {
                        // Indication of missing GPO for the number. Probably we passed the last one.
                        break;
                    }

                    audioStatus.Gpos.Add(gpoEnable.Value);
                }
            }

            return audioStatus;
        }

        public override async Task<bool> SetInputEnabledAsync(string hostAddress, int input, bool enabled)
        {
            // Fungerar endast på Quantum-kodare, ej Quantum ST
            var cmd = new CommandIkusNetSetInputEnabled { Input = input, Enabled = enabled };
            return await SendConfigurationCommandAsync(hostAddress, cmd);
        }

        public override async Task<int> SetInputGainLevelAsync(string hostAddress, int input, int gainLevel)
        {
            // Fungerar endast på Quantum-kodare, ej Quantum ST
            var cmd = new CommandIkusNetSetInputGainLevel { GainLeveldB = gainLevel, Input = input };
            await SendConfigurationCommandAsync(hostAddress, cmd);
            return gainLevel; // TODO: Check value and return real input level
        }

    }
}