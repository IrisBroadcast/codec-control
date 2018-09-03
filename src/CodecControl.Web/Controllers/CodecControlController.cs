using System;
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
        private readonly ICodecManager _codecManager;
        private readonly ICcmService _ccmService;

        private readonly string No_Gpo_Found = "Ingen GPO kunde hittas";

        public CodecControlController(ICcmService ccmService, ICodecManager codecManager)
        {
            _ccmService = ccmService;
            _codecManager = codecManager;
        }
        #endregion

        [Route("CheckCodecAvailable")]
        [HttpGet]
        public async Task<bool> CheckCodecAvailable(string sipAddress)
        {
            var codecInformation = GetCodecInformationBySipAddress(sipAddress);
            return await _codecManager.CheckIfAvailableAsync(codecInformation);
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

            if (codecInformation == null)
            {
                return NotFound();
            }

            var model = new AvailableGposViewModel();

            try
            {
                for (int i = 0; i < nrOfGpos; i++)
                {
                    bool? active = await _codecManager.GetGpoAsync(codecInformation, i);

                    if (!active.HasValue)
                    {
                        // GPO missing. Expected that we passed the last GPO
                        break;
                    }

                    var gpoName = GetGpoName(codecInformation.GpoNames, i);

                    model.Gpos.Add(new GpoViewModel()
                    {
                        Active = active.Value,
                        Name = string.IsNullOrWhiteSpace(gpoName) ? $"GPO {i}" : gpoName,
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

        public string GetGpoName(string gpoNames, int index)
        {
            var names = (gpoNames ?? string.Empty).Split(',');
            return index < names.Length ? names[index].Trim() : string.Empty;
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

            if (codecInformation == null)
            {
                return NotFound();
            }

            try
            {
                var audioStatus = await _codecManager.GetAudioStatusAsync(codecInformation, nrOfInputs, nrOfGpos);

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
                log.Warn(ex, "Exception when sending codec control command to " + codecInformation.SipAddress);
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

            if (codecInformation == null)
            {
                return NotFound();
            }

            try
            {
                var model = new InputGainAndStatusViewModel
                {
                    Enabled = await _codecManager.GetInputEnabledAsync(codecInformation, input),
                    GainLevel = await _codecManager.GetInputGainLevelAsync(codecInformation, input)
                };
                return model;
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when sending codec control command to " + codecInformation.SipAddress);
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

            if (codecInformation == null)
            {
                return NotFound();
            }

            try
            {
                var enabled = await _codecManager.GetInputEnabledAsync(codecInformation, input);
                return new InputStatusViewModel { Enabled = enabled };
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when sending codec control command to " + codecInformation.SipAddress);
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

            if (codecInformation == null)
            {
                return NotFound();
            }

            try
            {
                var model = new LineStatusViewModel();
                LineStatus lineStatus = await _codecManager.GetLineStatusAsync(codecInformation, line);

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
                log.Warn(ex, "Exception when sending codec control command to " + codecInformation.SipAddress);
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

            if (codecInformation == null)
            {
                return NotFound();
            }

            try
            {
                var loadedPreset = await _codecManager.GetLoadedPresetNameAsync(codecInformation, string.Empty);
                return new PresetViewModel { LoadedPreset = loadedPreset };
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when sending codec control command to " + codecInformation.SipAddress);
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

            if (codecInformation == null)
            {
                return NotFound();
            }

            try
            {
                var vuValues = await _codecManager.GetVuValuesAsync(codecInformation);

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
                log.Warn(ex, "Exception when sending codec control command to " + codecInformation.SipAddress);
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

            if (codecInformation == null)
            {
                return NotFound();
            }

            try
            {
                AudioMode result = await _codecManager.GetAudioModeAsync(codecInformation);

                return new AudioModeViewModel
                {
                    EncoderAudioMode = result.EncoderAudioAlgoritm,
                    DecoderAudioMode = result.DecoderAudioAlgoritm
                };
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when sending codec control command to " + codecInformation.SipAddress);
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

            if (codecInformation == null)
            {
                return NotFound();
            }

            try
            {
                await _codecManager.LoadPresetAsync(codecInformation, name);
                return true;
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when sending codec control command to " + codecInformation.SipAddress);
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

            if (codecInformation == null)
            {
                return NotFound();
            }

            try
            {
                await _codecManager.SetGpoAsync(codecInformation, number, active);
                var gpoActive = await _codecManager.GetGpoAsync(codecInformation, number) ?? false;
                return new GpoViewModel { Number = number, Active = gpoActive };
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when sending codec control command to " + codecInformation.SipAddress);
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

            if (codecInformation == null)
            {
                return NotFound();
            }

            try
            {
                var isEnabled = await _codecManager.SetInputEnabledAsync(codecInformation, input, enabled);
                return new InputStatusViewModel { Enabled = isEnabled };
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when sending codec control command to " + codecInformation.SipAddress);
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

            if (codecInformation == null)
            {
                return NotFound();
            }
         
            try
            {
                var gainLevel = await _codecManager.SetInputGainLevelAsync(codecInformation, input, level);
                return new InputGainLevelViewModel { GainLevel = gainLevel };
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Exception when sending codec control command to " + codecInformation.SipAddress);
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

            if (codecInformation == null)
            {
                return NotFound();
            }

            return await _codecManager.RebootAsync(codecInformation);
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

            if (codecInformation == null)
            {
                return NotFound();
            }

            return await _codecManager.CallAsync(codecInformation, callee, profileName);
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

            if (codecInformation == null)
            {
                return NotFound();
            }

            return await _codecManager.HangUpAsync(codecInformation);
        }

        private CodecInformation GetCodecInformationBySipAddress(string sipAddress)
        {
            var codecInfo = _ccmService.GetCodecInformationBySipAddress(sipAddress);
            return string.IsNullOrEmpty(codecInfo?.Api) ? null : codecInfo;
        }

    }

}
