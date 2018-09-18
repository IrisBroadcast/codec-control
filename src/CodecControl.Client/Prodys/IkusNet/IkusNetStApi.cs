using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodecControl.Client.Models;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;
using CodecControl.Client.Prodys.IkusNet.Sdk.Responses;

namespace CodecControl.Client.Prodys.IkusNet
{
    // This API is intended for Quantum ST that lacks controllable inputs.

    public class IkusNetStApi : IkusNetApiBase, ICodecApi
    {

        public IkusNetStApi(SocketPool socketPool) :base(socketPool)
        {
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
            catch (Exception)
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

        public virtual Task<bool> GetInputEnabledAsync(string hostAddress, int input)
        {
            // Not implemented in Quantum ST
            throw new NotImplementedException();
        }

        public virtual Task<int> GetInputGainLevelAsync(string hostAddress, int input)
        {
            // Not implemented in Quantum ST
            throw new NotImplementedException();
        }

        public virtual Task<(bool, int)> GetInputGainAndStatusAsync(string hostAddress, int input)
        {
            // Not implemented in Quantum ST
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
                    StatusCode = IkusNetMapper.MapToLineStatus(response.LineStatus),
                    DisconnectReason = IkusNetMapper.MapToDisconnectReason(response.DisconnectionCode)
                };
            }
        }
        
        public async Task<VuValues> GetVuValuesAsync(string hostAddress)
        {
            using (var socket = await SocketPool.TakeSocket(hostAddress))
            {
                SendCommand(socket, new CommandIkusNetGetVuMeters());
                var response = new IkusNetGetVumetersResponse(socket);
                return IkusNetMapper.MapToVuValues(response);
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
                    EncoderAudioAlgoritm = IkusNetMapper.MapToAudioAlgorithm(encoderResponse.AudioAlgorithm),
                    DecoderAudioAlgoritm = IkusNetMapper.MapToAudioAlgorithm(decoderResponse.AudioAlgorithm)
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
                audioStatus.VuValues = IkusNetMapper.MapToVuValues(vuResponse);

                // // Input status not implemented in Quantum ST
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

        public async Task<bool> RebootAsync(string hostAddress)
        {
            var cmd = new CommandIkusNetReboot();
            return await SendConfigurationCommandAsync(hostAddress, cmd);
        }

        public async Task<bool> SetGpoAsync(string hostAddress, int gpo, bool active)
        {
            var cmd = new CommandIkusNetSetGpo { Active = active, Gpo = gpo };
            return await SendConfigurationCommandAsync(hostAddress, cmd);
        }

        public virtual Task<bool> SetInputEnabledAsync(string hostAddress, int input, bool enabled)
        {
            // Not implemented in Quantum ST
            throw new NotImplementedException();
        }

        public virtual Task<int> SetInputGainLevelAsync(string hostAddress, int input, int gainLevel)
        {
            // Not implemented in Quantum ST
            throw new NotImplementedException();
        }
        #endregion

    }
}