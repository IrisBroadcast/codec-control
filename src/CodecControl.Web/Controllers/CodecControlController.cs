using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Client.Models;
using CodecControl.Web.Interfaces;
using CodecControl.Web.Models;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace CodecControl.Web.Controllers
{
    public class CodecControlController : ApiControllerBase
    {
        #region Constructor and members
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly CodecApiFactory _codecApiFactory;
        private readonly ICcmService _ccmService;

        private readonly string No_Gpo_Found = "Ingen GPO kunde hittas";

        public CodecControlController(ICcmService ccmService, CodecApiFactory codecApiFactory)
        {
            _ccmService = ccmService;
            _codecApiFactory = codecApiFactory;
        }
        #endregion

        [Route("CheckCodecAvailable")]
        [HttpGet]
        public async Task<ActionResult<bool>> CheckCodecAvailable(string sipAddress)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                return BadRequest();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                return CodecUnavailable();
            }

            return await codecApi.CheckIfAvailableAsync(sipAddress);
        }

        [Route("GetAvailableGpos")]
        [HttpPost]
        public async Task<ActionResult<AvailableGposViewModel>> GetAvailableGpos(string sipAddress, int nrOfGpos = 10)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                return BadRequest();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                return CodecUnavailable();
            }

            string gpoNameString = codecInformation.GpoNames;
            List<string> gpoNames = (gpoNameString ?? string.Empty).Split(',').Select(s => s.Trim()).ToList();

            var model = new AvailableGposViewModel();

            try
            {
                for (int i = 0; i < nrOfGpos; i++)
                {
                    bool? active = await codecApi.GetGpoAsync(sipAddress, i);

                    if (!active.HasValue)
                    {
                        // GPO missing. Expected that we passed the last GPO
                        break;
                    }

                    model.Gpos.Add(new GpoViewModel()
                    {
                        Active = active.Value,
                        Name = i < gpoNames.Count ? gpoNames[i] : $"GPO {i}",
                        Number = i
                    });
                }
            }
            catch (Exception ex)
            {
                if (model.Gpos.Count == 0)
                {
                    model.Error = No_Gpo_Found;
                }
            }

            return model;
        }

        [Route("GetAudioStatus")]
        [HttpGet]
        public async Task<ActionResult<AudioStatusViewModel>> GetAudioStatus(string sipAddress, int nrOfInputs = 2, int nrOfGpos = 2)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                return BadRequest();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                return CodecUnavailable();
            }

            try
            {
                var audioStatus = await codecApi.GetAudioStatusAsync(sipAddress, nrOfInputs, nrOfGpos);

                var model = new AudioStatusViewModel()
                {
                    Gpos = audioStatus.Gpos,
                    InputStatuses = audioStatus.InputStatuses,
                    VuValues = audioStatus.VuValues
                };
                return model;
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when sending codec control command to " + sipAddress);
                return CodecUnavailable();
            }
        }

        [Route("GetInputGainAndStatus")]
        [HttpGet]
        public async Task<ActionResult<InputGainAndStatusViewModel>> GetInputGainAndStatus(string sipAddress, int input)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                return BadRequest();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                return CodecUnavailable();
            }

            try
            {
                var model = new InputGainAndStatusViewModel
                {
                    Enabled = await codecApi.GetInputEnabledAsync(sipAddress, input),
                    GainLevel = await codecApi.GetInputGainLevelAsync(sipAddress, input)
                };
                return model;
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when sending codec control command to " + sipAddress);
                return CodecUnavailable();
            }
        }

        [Route("GetInputStatus")]
        [HttpGet]
        public async Task<ActionResult<InputStatusViewModel>> GetInputStatus(string sipAddress, int input)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                return BadRequest();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                return CodecUnavailable();
            }

            try
            {
                var enabled = await codecApi.GetInputEnabledAsync(sipAddress, input);
                return new InputStatusViewModel { Enabled = enabled };
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when sending codec control command to " + sipAddress);
                return CodecUnavailable();
            }
        }

        [Route("GetLineStatus")]
        [HttpGet]
        public async Task<ActionResult<LineStatusViewModel>> GetLineStatus(string sipAddress, int line)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                return BadRequest();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                return CodecUnavailable();
            }

            try
            {
                var model = new LineStatusViewModel();
                LineStatus lineStatus = await codecApi.GetLineStatusAsync(sipAddress, line);

                if (lineStatus == null || lineStatus.StatusCode == LineStatusCode.ErrorGettingStatus)
                {
                    model.Error = LineStatusCode.ErrorGettingStatus.ToString();
                }
                else
                {
                    model.Status = lineStatus.StatusCode.ToString();
                    model.DisconnectReason = lineStatus.DisconnectReason;
                    model.RemoteAddress = lineStatus.RemoteAddress;
                    model.LineStatus = lineStatus.StatusCode;
                }
                return model;
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when sending codec control command to " + sipAddress);
                return CodecUnavailable();
            }
        }

        [Route("GetLoadedPreset")]
        [HttpPost]
        public async Task<ActionResult<PresetViewModel>> GetLoadedPreset(string sipAddress)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                return BadRequest();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                return CodecUnavailable();
            }

            try
            {
                var loadedPreset = await codecApi.GetLoadedPresetNameAsync(sipAddress, string.Empty);
                return new PresetViewModel { LoadedPreset = loadedPreset };
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when sending codec control command to " + sipAddress);
                return CodecUnavailable();
            }
        }

        [Route("GetVuValues")]
        [HttpGet]
        public async Task<ActionResult<VuValuesViewModel>> GetVuValues(string sipAddress)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                return BadRequest();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                return CodecUnavailable();
            }

            try
            {
                var vuValues = await codecApi.GetVuValuesAsync(sipAddress);

                return new VuValuesViewModel
                {
                    RxLeft = vuValues.RxLeft,
                    RxRight = vuValues.RxRight,
                    TxLeft = vuValues.TxLeft,
                    TxRight = vuValues.TxRight
                };
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when sending codec control command to " + sipAddress);
                return CodecUnavailable();
            }
        }

        [Route("GetAudioMode")]
        [HttpGet]
        public async Task<ActionResult<AudioModeViewModel>> GetAudioMode(string sipAddress)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                return BadRequest();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                return CodecUnavailable();
            }

            try
            {
                AudioMode result = await codecApi.GetAudioModeAsync(sipAddress);

                return new AudioModeViewModel
                {
                    EncoderAudioMode = result.EncoderAudioAlgoritm,
                    DecoderAudioMode = result.DecoderAudioAlgoritm
                };
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when sending codec control command to " + sipAddress);
                return CodecUnavailable();
            }
        }

        [Route("LoadPreset")]
        [HttpPost]
        public async Task<ActionResult<bool>> LoadPreset(string sipAddress, string name)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                return BadRequest();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                return CodecUnavailable();
            }

            try
            {
                await codecApi.LoadPresetAsync(sipAddress, name);
                return true;
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when sending codec control command to " + sipAddress);
                return CodecUnavailable();
            }
        }

        [Route("SetGpo")]
        [HttpGet]
        public async Task<ActionResult<GpoViewModel>> SetGpo(string sipAddress, int number, bool active)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                return BadRequest();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                return CodecUnavailable();
            }

            try
            {
                await codecApi.SetGpoAsync(sipAddress, number, active);
                var gpoActive = await codecApi.GetGpoAsync(sipAddress, number) ?? false;
                return new GpoViewModel { Number = number, Active = gpoActive };
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when sending codec control command to " + sipAddress);
                return CodecUnavailable();
            }
        }

        [Route("SetInputEnabled")]
        [HttpPost]
        public async Task<ActionResult<InputStatusViewModel>> SetInputEnabled(string sipAddress, int input, bool enabled)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                return BadRequest();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                return CodecUnavailable();
            }

            try
            {
                var isEnabled = await codecApi.SetInputEnabledAsync(sipAddress, input, enabled);
                return new InputStatusViewModel { Enabled = isEnabled };
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when sending codec control command to " + sipAddress);
                return CodecUnavailable();
            }
        }

        [HttpPost]
        [Route("SetInputGainLevel")]
        public async Task<ActionResult<InputGainLevelViewModel>> SetInputGainLevelAsync(string sipAddress, int input, int level)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                return BadRequest();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                return CodecUnavailable();
            }

            try
            {
                var gainLevel = await codecApi.SetInputGainLevelAsync(sipAddress, input, level);
                return new InputGainLevelViewModel { GainLevel = gainLevel };
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when sending codec control command to " + sipAddress);
                return CodecUnavailable();
            }
        }

        [Route("RebootCodec")]
        [HttpPost]
        public async Task<ActionResult<bool>> RebootCodec(string sipAddress)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                return BadRequest();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                return CodecUnavailable();
            }

            return await codecApi.RebootAsync(sipAddress);
        }

        [Route("Call")]
        [HttpPost]
        public async Task<ActionResult<bool>> Call(string sipAddress, string callee, string profileName)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                return BadRequest();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                return CodecUnavailable();
            }

            try
            {
                return await codecApi.CallAsync(sipAddress, callee, profileName);
            }
            catch (Exception ex)
            {
                return CodecUnavailable();
            }
        }


        [Route("Hangup")]
        [HttpPost]
        public async Task<ActionResult<bool>> Hangup(string sipAddress)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                return BadRequest();
            }

            CodecInformation codecInformation = GetCodecInformationBySipAddress(sipAddress);
            var codecApi = _codecApiFactory.CreateCodecApi(codecInformation?.CodecApiType);

            if (codecInformation == null || codecApi == null)
            {
                return CodecUnavailable();
            }

            try
            {
                return await codecApi.HangUpAsync(sipAddress);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        private CodecInformation GetCodecInformationBySipAddress(string sipAddress)
        {
            var codecInfo = _ccmService.GetCodecInformationBySipAddress(sipAddress);
            return codecInfo?.CodecApiType == null ? null : codecInfo;
        }

    }

}
