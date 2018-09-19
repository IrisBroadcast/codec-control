using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CodecControl.Client.Models;
using NLog;

namespace CodecControl.Client.SR.BaresipRest
{
    /// <summary>
    /// Codec control implementation for the Baresip, this implementation is experimental/proprietary
    /// </summary>
    public class BaresipRestApi : ICodecApi
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        public async Task<bool> CheckIfAvailableAsync(string ip)
        {
            // Connect to the unit and check for response on API:port
            var url = CreateUrl(ip, "/api/isavailable");

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(4);
                var response = await client.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
        }

        public async Task<bool> CallAsync(string ip, string callee, string profileName)
        {
            var url = CreateUrl(ip, "api/call");
            var response = await HttpService.PostJsonAsync(url, new { address = callee });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> HangUpAsync(string ip)
        {
            var url = CreateUrl(ip, "api/hangup");
            var response = await HttpService.PostJsonAsync(url);
            return response.IsSuccessStatusCode;
        }

        public Task<bool?> GetGpoAsync(string ip, int gpio) { throw new NotImplementedException(); }

        public async Task<bool> GetInputEnabledAsync(string ip, int input)
        {
            var url = CreateUrl(ip, "api/inputenable?input=" + (input + 1) );
            var gainObject = await HttpService.GetAsync<InputEnableResponse>(url);
            return gainObject?.Value ?? false;
        }

        public async Task<int> GetInputGainLevelAsync(string ip, int input)
        {
            var url = CreateUrl(ip, "api/inputgain?input=" + (input + 1));
            var gainObject = await HttpService.GetAsync<InputGainResponse>(url);
            return gainObject?.Value ?? 0;
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
            var lineStatus = await HttpService.GetAsync<BaresipLineStatus>(url);

            return new LineStatus
            {
                DisconnectReason = BaresipMapper.MapToDisconnectReason(lineStatus.Call.Code),
                StatusCode = BaresipMapper.MapToLineStatusCode(lineStatus.State)
            };
        }

        public Task<VuValues> GetVuValuesAsync(string ip)
        {
            throw new NotImplementedException();
        }

        public Task<AudioMode> GetAudioModeAsync(string ip)
        {
            throw new NotImplementedException();
        }
        
        public async Task<AudioStatus> GetAudioStatusAsync(string ip, int nrOfInputs, int nrOfGpos)
        {
            return await GetAudioStatusAsync(ip);
        }

        private async Task<AudioStatus> GetAudioStatusAsync(string ip)
        {
            var url = CreateUrl(ip, "api/audiostatus");
            var bareSipAudioStatus = await HttpService.GetAsync<BaresipAudioStatus>(url);

            try
            {
                var audioStatus = new AudioStatus()
                {
                    Gpos = bareSipAudioStatus.Control.Gpo.Select(gpo => gpo.Active).ToList(),
                    InputStatus = bareSipAudioStatus.Inputs.Select(BaresipMapper.MapToInputStatus).ToList(),
                    VuValues = BaresipMapper.MapToVuValues(bareSipAudioStatus)
                };
                return audioStatus;
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when converting audio status");
                return null;
            }
        }

        public Task<bool> SetGpoAsync(string ip, int gpo, bool active)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SetInputEnabledAsync(string ip, int input, bool enabled)
        {
            var url = CreateUrl(ip, "api/inputenable");
            var response = await HttpService.PostJsonAsync(url, new { input = input, value = enabled });
            return response.IsSuccessStatusCode;
        }

        public async Task<int> SetInputGainLevelAsync(string ip, int input, int gainLevel)
        {
            var url = CreateUrl(ip, "api/inputgain");
            var response = await HttpService.PostJsonAsync(url, new { input = input, value = gainLevel});
            return gainLevel; // TODO: Return real input level
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

    public abstract class BaresipResponse
    {
        public bool Success { get; set; }
    }

    public class InputGainResponse : BaresipResponse
{
        public int Value { get; set; }
    }

    public class InputEnableResponse : BaresipResponse
    {
        public bool Value { get; set; }
    }

}