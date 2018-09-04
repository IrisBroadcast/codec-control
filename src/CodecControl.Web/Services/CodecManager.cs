using System;
using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Client.Exceptions;
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
            return codecInfo?.CodecApiType == null ? null : codecInfo;
        }


        public async Task<bool> CallAsync(string sipAddress, string callee, string profileName)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                throw new MissingSipAddressException();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);

            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                throw new CodecApiNotFoundException();
            }

            // TODO: first check codec call status. Do not execute the call method if the codec is already in a call.
            // Some codecs will hangup the current call and dial up the new call without hesitation.
            var callAsync = await codecApi.CallAsync(codecInformation.Ip, callee, profileName);
            return callAsync;
        }

        public async Task<bool> HangUpAsync(string sipAddress)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                throw new MissingSipAddressException();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);

            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                throw new CodecApiNotFoundException();
            }

            return await codecApi.HangUpAsync(codecInformation.Ip);
        }

        public async Task<bool> CheckIfAvailableAsync(string sipAddress)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                throw new MissingSipAddressException();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);

            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                throw new CodecApiNotFoundException();
            }

            return await codecApi.CheckIfAvailableAsync(codecInformation.Ip);
        }

        public async Task<bool?> GetGpoAsync(string sipAddress, int gpio)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                throw new MissingSipAddressException();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);

            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                throw new CodecApiNotFoundException();
            }

            return await codecApi.GetGpoAsync(codecInformation.Ip, gpio);
        }



        public async Task<bool> SetGpoAsync(string sipAddress, int gpo, bool active)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                throw new MissingSipAddressException();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);

            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                throw new CodecApiNotFoundException();
            }

            return await codecApi.SetGpoAsync(codecInformation.Ip, gpo, active);
        }

        public async Task<bool> SetInputEnabledAsync(string sipAddress, int input, bool enabled)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                throw new MissingSipAddressException();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);

            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                throw new CodecApiNotFoundException();
            }

            return await codecApi.SetInputEnabledAsync(codecInformation.Ip, input, enabled);
        }

        public async Task<int> SetInputGainLevelAsync(string sipAddress, int input, int gainLevel)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                throw new MissingSipAddressException();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);

            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                throw new CodecApiNotFoundException();
            }

            await codecApi.SetInputGainLevelAsync(codecInformation.Ip, input, gainLevel);
            return await codecApi.GetInputGainLevelAsync(codecInformation.Ip, input);
        }

        public async Task<bool> GetInputEnabledAsync(string sipAddress, int input)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                throw new MissingSipAddressException();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);

            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                throw new CodecApiNotFoundException();
            }

            return await codecApi.GetInputEnabledAsync(codecInformation.Ip, input);
        }

        public async Task<int> GetInputGainLevelAsync(string sipAddress, int input)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                throw new MissingSipAddressException();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);

            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                throw new CodecApiNotFoundException();
            }

            return await codecApi.GetInputGainLevelAsync(codecInformation.Ip, input);
        }

        public async Task<LineStatus> GetLineStatusAsync(string sipAddress, int line)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                throw new MissingSipAddressException();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);

            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                throw new CodecApiNotFoundException();
            }

            return await codecApi.GetLineStatusAsync(codecInformation.Ip, line);
        }

        public async Task<string> GetLoadedPresetNameAsync(string sipAddress, string lastPresetName)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                throw new MissingSipAddressException();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);

            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                throw new CodecApiNotFoundException();
            }

            return await codecApi.GetLoadedPresetNameAsync(codecInformation.Ip, lastPresetName);
        }

        public async Task<VuValues> GetVuValuesAsync(string sipAddress)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                throw new MissingSipAddressException();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);

            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                throw new CodecApiNotFoundException();
            }

            return await codecApi.GetVuValuesAsync(codecInformation.Ip);
        }

        public async Task<AudioStatus> GetAudioStatusAsync(string sipAddress, int nrOfInputs, int nrOfGpos)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                throw new MissingSipAddressException();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);

            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                throw new CodecApiNotFoundException();
            }

            return await codecApi.GetAudioStatusAsync(codecInformation.Ip, nrOfInputs, nrOfGpos);
        }

        public async Task<AudioMode> GetAudioModeAsync(string sipAddress)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                throw new MissingSipAddressException();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);

            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                throw new CodecApiNotFoundException();
            }

            return await codecApi.GetAudioModeAsync(codecInformation.Ip);
        }

        public async Task<bool> LoadPresetAsync(string sipAddress, string preset)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                throw new MissingSipAddressException();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);

            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                throw new CodecApiNotFoundException();
            }

            return await codecApi.LoadPresetAsync(codecInformation.Ip, preset);
        }

        public async Task<bool> RebootAsync(string sipAddress)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                throw new MissingSipAddressException();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);

            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                throw new CodecApiNotFoundException();
            }

            return await codecApi.RebootAsync(codecInformation.Ip);
        }

    }
}