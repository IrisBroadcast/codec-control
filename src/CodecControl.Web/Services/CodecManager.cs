using System;
using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Client.Models;
using CodecControl.Web.Controllers;
using CodecControl.Web.Interfaces;
using NLog;

namespace CodecControl.Web.Services
{
    /// <summary>
    /// Manager for connecting with Code APIs
    /// </summary>
    public class CodecManager : ICodecManager
    {
        private readonly CodecApiFactory _codecApiFactory;
        private readonly ICcmService _ccmService;
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        public CodecManager(CodecApiFactory codecApiFactory, ICcmService ccmService)
        {
            _codecApiFactory = codecApiFactory;
            _ccmService = ccmService;
        }

        private CodecInformation GetCodecInformationBySipAddress(string sipAddress)
        {
            var codecInfo = _ccmService.GetCodecInformationBySipAddress(sipAddress);
            return string.IsNullOrEmpty(codecInfo?.Api) ? null : codecInfo;
        }

        //private ICodecApi CreateCodecApi(CodecInformation codecInformation)
        //{
        //    if (codecInformation == null)
        //    {
        //        throw new CodecApiNotFoundException("Missing codec api information.");
        //    }

        //    switch (codecInformation.Api)
        //    {
        //        case "IkusNet":
        //            return new IkusNetApi();
        //        case "BaresipRest":
        //            return new BaresipRestApi();
        //        default:
        //            throw new CodecApiNotFoundException($"Could not load API {codecInformation.Api}.");
        //    }
        //}


        public async Task<bool> CallAsync(string sipAddress, string callee, string profileName)
        {
            // TODO: first check codec call status. Do not execute the call method if the codec is already in a call.
            // Some codecs will hangup the current call and dial up the new call without hesitation.

            if (string.IsNullOrEmpty(sipAddress))
            {
                throw new ArgumentException(nameof(sipAddress));
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);

            if (codecInformation == null)
            {
                throw new Exception("");
            }
            
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation);

            if (codecApi == null)
            {
                throw new Exception();
            }

            return await codecApi.CallAsync(codecInformation.Ip, callee, profileName);
        }

        public async Task<bool> HangUpAsync(CodecInformation codecInformation)
        {
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation);
            return await codecApi.HangUpAsync(codecInformation.Ip);
        }

        public async Task<bool> CheckIfAvailableAsync(CodecInformation codecInformation)
        {
            try
            {
                var codecApi = _codecApiFactory.CreateCodecApi(codecInformation);
                return await codecApi.CheckIfAvailableAsync(codecInformation.Ip);
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception in CheckIfAvailableAsync");
                return false;
            }
        }

        public async Task<bool?> GetGpoAsync(CodecInformation codecInformation, int gpio)
        {
            try
            {
                var codecApi = _codecApiFactory.CreateCodecApi(codecInformation);
                var gpo = await codecApi.GetGpoAsync(codecInformation.Ip, gpio);
                return gpo;

            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> GetInputEnabledAsync(CodecInformation codecInformation, int input)
        {
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation);
            return await codecApi.GetInputEnabledAsync(codecInformation.Ip, input);
        }

        public async Task<int> GetInputGainLevelAsync(CodecInformation codecInformation, int input)
        {
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation);
            return await codecApi.GetInputGainLevelAsync(codecInformation.Ip, input);
        }

        public async Task<LineStatus> GetLineStatusAsync(CodecInformation codecInformation, int line)
        {
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation);
            return await codecApi.GetLineStatusAsync(codecInformation.Ip, line);
        }

        public async Task<string> GetLoadedPresetNameAsync(CodecInformation codecInformation, string lastPresetName)
        {
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation);
            return await codecApi.GetLoadedPresetNameAsync(codecInformation.Ip, lastPresetName);
        }

        public async Task<VuValues> GetVuValuesAsync(CodecInformation codecInformation)
        {
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation);
            return await codecApi.GetVuValuesAsync(codecInformation.Ip);
        }

        public async Task<AudioStatus> GetAudioStatusAsync(CodecInformation codecInformation, int nrOfInputs, int nrOfGpos)
        {
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation);
            return await codecApi.GetAudioStatusAsync(codecInformation.Ip, nrOfInputs, nrOfGpos);
        }

        public async Task<AudioMode> GetAudioModeAsync(CodecInformation codecInformation)
        {
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation);
            return await codecApi.GetAudioModeAsync(codecInformation.Ip);
        }

        public async Task<bool> LoadPresetAsync(CodecInformation codecInformation, string preset)
        {
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation);
            return await codecApi.LoadPresetAsync(codecInformation.Ip, preset);
        }

        public async Task<bool> RebootAsync(CodecInformation codecInformation)
        {
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation);
            return await codecApi.RebootAsync(codecInformation.Ip);
        }

        public async Task<bool> SetGpoAsync(CodecInformation codecInformation, int gpo, bool active)
        {
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation);
            return await codecApi.SetGpoAsync(codecInformation.Ip, gpo, active);
        }

        public async Task<bool> SetInputEnabledAsync(CodecInformation codecInformation, int input, bool enabled)
        {
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation);
            return await codecApi.SetInputEnabledAsync(codecInformation.Ip, input, enabled);
        }

        public async Task<int> SetInputGainLevelAsync(CodecInformation codecInformation, int input, int gainLevel)
        {
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation);
            await codecApi.SetInputGainLevelAsync(codecInformation.Ip, input, gainLevel);
            return await codecApi.GetInputGainLevelAsync(codecInformation.Ip, input);
        }

    }
}