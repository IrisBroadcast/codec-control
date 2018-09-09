using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodecControl.Client.Models;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands.Base;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;
using CodecControl.Client.Prodys.IkusNet.Sdk.Responses;
using NLog;

namespace CodecControl.Client.Prodys.IkusNet
{
    // This API is intended for Quantum ST that lacks controllable inputs.
    public class IkusNetBaseApi : ICodecApi
    {
        protected readonly SocketPool SocketPool;
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        public IkusNetBaseApi(SocketPool socketPool)
        {
            SocketPool = socketPool;
        }

        public async Task<bool> CheckIfAvailableAsync(string ip)
        {
            try
            {
                using (var socket = await SocketPool.TakeSocket(ip))
                {
                    // Send dummy command to codec.
                    SendCommand(socket, new CommandIkusNetGetVuMeters());
                    var dummResponse = new IkusNetGetVumetersResponse(socket);
                    return true; // Success
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #region Get Commands

        // For test only
        public async Task<string> GetDeviceNameAsync(string hostAddress)
        {
            using (var socket = await SocketPool.TakeSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetSysGetDeviceName());
                var response = new IkusNetGetDeviceNameResponse(socket);
                return response.DeviceName;
            }
        }

        public async Task<bool?> GetGpiAsync(string hostAddress, int gpio)
        {
            using (var socket = await SocketPool.TakeSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetGpi { Gpio = gpio });
                var response = new IkusNetGetGpiResponse(socket);
                return response.Active;
            }
        }

        public async Task<bool?> GetGpoAsync(string hostAddress, int gpio)
        {
            using (var socket = await SocketPool.TakeSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetGpo { Gpio = gpio });
                var response = new IkusNetGetGpoResponse(socket);
                return response.Active;
            }
        }

        public virtual async Task<bool> GetInputEnabledAsync(string hostAddress, int input)
        {
            // Works only on Quantum codec, not Quantum ST
            throw new NotImplementedException();
        }

        public virtual async Task<int> GetInputGainLevelAsync(string hostAddress, int input)
        {
            // Works only on Quantum codec, not Quantum ST
            throw new NotImplementedException();
        }

        public virtual async Task<(bool, int)> GetInputGainAndStatusAsync(string hostAddress, int input)
        {
            // Works only on Quantum codec, not Quantum ST
            throw new NotImplementedException();
        }

        public async Task<LineStatus> GetLineStatusAsync(string hostAddress, int line)
        {
            using (var socket = await SocketPool.TakeSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetLineStatus { Line = (IkusNetLine)line });
                var response = new IkusNetGetLineStatusResponse(socket);

                return new LineStatus
                {
                    RemoteAddress = response.Address,
                    StatusCode = (LineStatusCode)response.LineStatus,
                    DisconnectReason = (DisconnectReason)response.DisconnectionCode,
                };
            }
        }

        public async Task<string> GetLoadedPresetNameAsync(string hostAddress, string lastPresetName)
        {
            using (var socket = await SocketPool.TakeSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetLoadedPresetName { LastLoadedPresetName = lastPresetName });
                var response = new IkusNetGetLoadedPresetNameResponse(socket);
                return response.PresetName;
            }
        }

        public async Task<VuValues> GetVuValuesAsync(string hostAddress)
        {
            using (var socket = await SocketPool.TakeSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetVuMeters());
                var response = new IkusNetGetVumetersResponse(socket);
                return new VuValues
                {
                    TxLeft = response.ProgramTxLeft,
                    TxRight = response.ProgramTxRight,
                    RxLeft = response.ProgramRxLeft,
                    RxRight = response.ProgramRxRight
                };
            }
        }

        public async Task<AudioMode> GetAudioModeAsync(string hostAddress)
        {
            using (var socket = await SocketPool.TakeSocket(hostAddress))
            {
                // Get encoder algoritm
                SendCommand(socket, new CommandIkusNetGetEncoderAudioMode());
                var encoderResponse = IkusNetGetEncoderAudioModeResponse.GetResponse(socket);

                // Get decoder algoritm
                SendCommand(socket, new CommandIkusNetGetDecoderAudioMode());
                var decoderResponse = IkusNetGetDecoderAudioModeResponse.GetResponse(socket);

                return new AudioMode
                {
                    EncoderAudioAlgoritm = (AudioAlgorithm)encoderResponse.AudioAlgorithm,
                    DecoderAudioAlgoritm = (AudioAlgorithm)decoderResponse.AudioAlgorithm
                };
            }
        }

        public virtual async Task<AudioStatus> GetAudioStatusAsync(string hostAddress, int nrOfInputs, int nrOfGpos)
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


                // Works only on Quantum codec, not Quantum ST
                audioStatus.InputStatuses = new List<InputStatus>();

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

        #endregion

        #region Configuration Commands
        public async Task<bool> CallAsync(string hostAddress, string callee, string profileName)
        {
            // TODO: first check codec call status. Do not execute the call method if the codec is already in a call.
            // Some codecs will hangup the current call and dial up the new call without hesitation.

            var cmd = new CommandIkusNetCall
            {
                Address = callee,
                Profile = profileName,
                CallContent = IkusNetCallContent.Audio,
                CallType = IkusNetIPCallType.UnicastBidirectional,
                Codec = IkusNetCodec.Program
            };
            return await SendConfigurationCommandAsync(hostAddress, cmd);
        }

        public async Task<bool> HangUpAsync(string hostAddress)
        {
            var cmd = new CommandIkusNetHangUp { Codec = IkusNetCodec.Program };
            return await SendConfigurationCommandAsync(hostAddress, cmd);
        }

        public async Task<bool> LoadPresetAsync(string hostAddress, string preset)
        {
            var cmd = new CommandIkusNetPresetLoad { PresetToLoad = preset };
            return await SendConfigurationCommandAsync(hostAddress, cmd);
        }

        public async Task<bool> RebootAsync(string hostAddress)
        {
            var cmd = new CommandIkusNetReboot();
            return await SendConfigurationCommandAsync(hostAddress, cmd);
        }

        public async Task<bool> SetDeviceName(string hostAddress, string newDeviceName)
        {
            var cmd = new CommandIkusNetSysSetDeviceName { DeviceName = newDeviceName };
            return await SendConfigurationCommandAsync(hostAddress, cmd);
        }

        public async Task<bool> SetGpoAsync(string hostAddress, int gpo, bool active)
        {
            var cmd = new CommandIkusNetSetGpo { Active = active, Gpo = gpo };
            return await SendConfigurationCommandAsync(hostAddress, cmd);
        }

        public virtual async Task<bool> SetInputEnabledAsync(string hostAddress, int input, bool enabled)
        {
            // Fungerar endast på Quantum-kodare, ej Quantum ST
            var cmd = new CommandIkusNetSetInputEnabled { Input = input, Enabled = enabled };
            return await SendConfigurationCommandAsync(hostAddress, cmd);
        }

        public virtual async Task<int> SetInputGainLevelAsync(string hostAddress, int input, int gainLevel)
        {
            // Fungerar endast på Quantum-kodare, ej Quantum ST
            var cmd = new CommandIkusNetSetInputGainLevel { GainLeveldB = gainLevel, Input = input };
            await SendConfigurationCommandAsync(hostAddress, cmd);
            return gainLevel; // TODO: Check value and return real input level
        }
        #endregion

        #region Protected methods 

        protected async Task<bool> SendConfigurationCommandAsync(string hostAddress, ICommandBase cmd)
        {
            using (var socket = await SocketPool.TakeSocket(hostAddress))
            {
                SendCommand(socket, cmd);
                var ackResponse = new AcknowledgeResponse(socket);
                return ackResponse.Acknowleged;
            }
        }

        protected int SendCommand(SocketProxy socket, ICommandBase command)
        {
            return socket.Send(command.GetBytes());
        }

        #endregion


    }
}