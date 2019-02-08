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
using System.Linq;
using System.Threading.Tasks;
using CodecControl.Client.Models;

namespace CodecControl.Client.SR.BaresipRest
{
    /// <summary>
    /// Codec control implementation for the Baresip, this implementation is experimental/proprietary
    /// </summary>
    public class BaresipRestApi : ICodecApi
    {
        public async Task<bool> CheckIfAvailableAsync(string ip)
        {
            var url = CreateUrl(ip, "/api/isavailable");
            var isAvailableResponse = await HttpService.GetWithBaresipResponseAsync<IsAvailableResponse>(url);
            return isAvailableResponse.Success;
        }

        public async Task<bool> CallAsync(string ip, string callee, string profileName, string deviceEncoder)
        {
            var url = CreateUrl(ip, "api/call");
            var response = await HttpService.PostWithBaresipResponseAsync<BaresipResponse>(url, new { address = callee });
            return response.Success;
        }

        public async Task<bool> HangUpAsync(string ip, string deviceEncoder)
        {
            var url = CreateUrl(ip, "api/hangup");
            var response = await HttpService.PostWithBaresipResponseAsync<BaresipResponse>(url);
            return response.Success;
        }

        public Task<bool?> GetGpoAsync(string ip, int gpio) { throw new NotImplementedException(); }

        public async Task<bool> GetInputEnabledAsync(string ip, int input)
        {
            var url = CreateUrl(ip, $"api/inputenable?input={input}");
            var inputEnableResponse = await HttpService.GetWithBaresipResponseAsync<InputEnableResponse>(url);
            return inputEnableResponse.Value;
        }

        public async Task<int> GetInputGainLevelAsync(string ip, int input)
        {
            var url = CreateUrl(ip, $"api/inputgain?input={input}");
            var inputGainResponse = await HttpService.GetWithBaresipResponseAsync<InputGainResponse>(url);
            return inputGainResponse.Value;
        }

        public async Task<(bool, int)> GetInputGainAndStatusAsync(string ip, int input)
        {
            var enabled = await GetInputEnabledAsync(ip, input);
            var gain = await GetInputGainLevelAsync(ip, input);
            return (enabled, gain);
        }

        public async Task<LineStatus> GetLineStatusAsync(string ip)
        {
            var url = CreateUrl(ip, "api/linestatus");
            var lineStatus = await HttpService.GetWithBaresipResponseAsync<BaresipLineStatus>(url);

            return new LineStatus
            {
                DisconnectReason = BaresipMapper.MapToDisconnectReason(lineStatus.Call.Code),
                StatusCode = BaresipMapper.MapToLineStatusCode(lineStatus.State),
                RemoteAddress = lineStatus.Call.RemoteAddress
            };
        }

        public async Task<VuValues> GetVuValuesAsync(string ip)
        {
            var url = CreateUrl(ip, "api/vuvalues");
            var vuValues = await HttpService.GetWithBaresipResponseAsync<BaresipVuValues>(url);

            return new VuValues
            {
                TxLeft = vuValues.Tx,
                TxRight = vuValues.Tx,
                RxLeft = vuValues.Rx,
                RxRight = vuValues.Rx
            };
        }

        public async Task<AudioMode> GetAudioModeAsync(string ip)
        {
            var url = CreateUrl(ip, "api/audioalgorithm");
            var audioAlgorithmResponse = await HttpService.GetWithBaresipResponseAsync<BaresipAudioAlgorithmResponse>(url);

            return new AudioMode
            {
                EncoderAudioAlgoritm = BaresipMapper.MapToAudioAlgorithm(audioAlgorithmResponse.EncoderAudioAlgoritm),
                DecoderAudioAlgoritm = BaresipMapper.MapToAudioAlgorithm(audioAlgorithmResponse.DecoderAudioAlgoritm)
            };
        }

        public async Task<AudioStatus> GetAudioStatusAsync(string ip, int nrOfInputs, int nrOfGpos)
        {
            var url = CreateUrl(ip, "api/audiostatus");
            var bareSipAudioStatus = await HttpService.GetWithBaresipResponseAsync<BaresipAudioStatus>(url);

            var audioStatus = new AudioStatus()
            {
                Gpos = bareSipAudioStatus.Control.Gpo.Select(gpo => new GpoStatus() { Index = gpo.Id, Active = gpo.Active }).ToList(),
                InputStatus = bareSipAudioStatus.Inputs.Select(BaresipMapper.MapToInputStatus).ToList(),
                VuValues = new VuValues() { TxLeft = -100, TxRight = -100, RxLeft = -100, RxRight = -100 } // Dummy VU values
            };
            return audioStatus;
        }

        public Task<bool> SetGpoAsync(string ip, int gpo, bool active)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SetInputEnabledAsync(string ip, int input, bool enabled)
        {
            var url = CreateUrl(ip, "api/inputenable");
            var response = await HttpService.PostWithBaresipResponseAsync<InputEnableResponse>(url, new { input = input, value = enabled });
            return response.Value;
        }

        public async Task<int> SetInputGainLevelAsync(string ip, int input, int gainLevel)
        {
            var url = CreateUrl(ip, "api/inputgain");
            var inputgainResponse = await HttpService.PostWithBaresipResponseAsync<InputGainResponse>(url, new { input = input, value = gainLevel });
            return inputgainResponse.Value;
        }

        public Task<bool> RebootAsync(string ip)
        {
            throw new NotImplementedException();
        }

        private Uri CreateUrl(string ip, string path)
        {
            var baseUrl = new Uri($"http://{ip}:{Sdk.Baresip.ExternalProtocolIpCommandsPort}");
            return new Uri(baseUrl, path);
        }
    }


}