using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Client.Exceptions;
using CodecControl.Client.Models;
using CodecControl.Web.Controllers.Base;
using CodecControl.Web.Interfaces;
using CodecControl.Web.Models;
using CodecControl.Web.Services;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace CodecControl.Web.Controllers
{
    public class CodecControlController : ApiControllerBase
    {
        #region Constructor and members
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IServiceProvider _serviceProvider;
        private readonly ICcmService _ccmService;

        private readonly string No_Gpo_Found = "Ingen GPO kunde hittas";

        public CodecControlController(ICcmService ccmService, IServiceProvider serviceProvider)
        {
            _ccmService = ccmService;
            _serviceProvider = serviceProvider;
        }
        #endregion

        [Route("CheckCodecAvailable")]
        [HttpGet]
        public async Task<ActionResult<bool>> CheckCodecAvailable(string sipAddress)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
                {
                    return await codecApi.CheckIfAvailableAsync(codecInformation.Ip); 
                });
        }

        [Route("GetAvailableGpos")]
        [HttpPost]
        public async Task<ActionResult<AvailableGposViewModel>> GetAvailableGpos(string sipAddress, int nrOfGpos = 10)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                string gpoNameString = codecInformation.GpoNames;
                List<string> gpoNames = (gpoNameString ?? string.Empty).Split(',').Select(s => s.Trim()).ToList();

                var model = new AvailableGposViewModel();

                try
                {
                    for (int i = 0; i < nrOfGpos; i++)
                    {
                        bool? active = await codecApi.GetGpoAsync(codecInformation.Ip, i);

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
                    // TODO: Log warning
                }

                return model;
            });
        }

        [Route("GetAudioStatus")]
        [HttpGet]
        public async Task<ActionResult<AudioStatusViewModel>> GetAudioStatus(string sipAddress, int nrOfInputs = 2, int nrOfGpos = 2)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                var audioStatus = await codecApi.GetAudioStatusAsync(codecInformation.Ip, nrOfInputs, nrOfGpos);

                var model = new AudioStatusViewModel()
                {
                    Gpos = audioStatus.Gpos,
                    InputStatuses = audioStatus.InputStatuses,
                    VuValues = audioStatus.VuValues
                };
                return model;
            });
        }


        [Route("GetInputGainAndStatus")]
        [HttpGet]
        public async Task<ActionResult<InputGainAndStatusViewModel>> GetInputGainAndStatus(string sipAddress, int input)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                var model = new InputGainAndStatusViewModel
                {
                    Enabled = await codecApi.GetInputEnabledAsync(codecInformation.Ip, input),
                    GainLevel = await codecApi.GetInputGainLevelAsync(codecInformation.Ip, input)
                };
                return model;
            });
        }

        [Route("GetInputStatus")]
        [HttpGet]
        public async Task<ActionResult<InputStatusViewModel>> GetInputStatus(string sipAddress, int input)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                var enabled = await codecApi.GetInputEnabledAsync(codecInformation.Ip, input);
                return new InputStatusViewModel { Enabled = enabled };
            });
        }

        [Route("GetLineStatus")]
        [HttpGet]
        public async Task<ActionResult<LineStatusViewModel>> GetLineStatus(string sipAddress, int line)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                var model = new LineStatusViewModel();
                LineStatus lineStatus = await codecApi.GetLineStatusAsync(codecInformation.Ip, line);

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
            });
        }

        [Route("GetLoadedPreset")]
        [HttpPost]
        public async Task<ActionResult<PresetViewModel>> GetLoadedPreset(string sipAddress)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                var loadedPreset = await codecApi.GetLoadedPresetNameAsync(codecInformation.Ip, string.Empty);
                return new PresetViewModel {LoadedPreset = loadedPreset};
            });

        }

        [Route("GetVuValues")]
        [HttpGet]
        public async Task<ActionResult<VuValuesViewModel>> GetVuValues(string sipAddress)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) => {
                var vuValues = await codecApi.GetVuValuesAsync(codecInformation.Ip);

                return new VuValuesViewModel
                {
                    RxLeft = vuValues.RxLeft,
                    RxRight = vuValues.RxRight,
                    TxLeft = vuValues.TxLeft,
                    TxRight = vuValues.TxRight
                };
            });
        }

        [Route("GetAudioMode")]
        [HttpGet]
        public async Task<ActionResult<AudioModeViewModel>> GetAudioMode(string sipAddress)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) => {
                AudioMode result = await codecApi.GetAudioModeAsync(codecInformation.Ip);

                return new AudioModeViewModel
                {
                    EncoderAudioMode = result.EncoderAudioAlgoritm,
                    DecoderAudioMode = result.DecoderAudioAlgoritm
                };
            });
        }

        [Route("LoadPreset")]
        [HttpPost]
        public async Task<ActionResult<bool>> LoadPreset(string sipAddress, string name)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                await codecApi.LoadPresetAsync(codecInformation.Ip, name);
                return true;
            });
        }

        [Route("SetGpo")]
        [HttpGet]
        public async Task<ActionResult<GpoViewModel>> SetGpo(string sipAddress, int number, bool active)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                await codecApi.SetGpoAsync(codecInformation.Ip, number, active);
                var gpoActive = await codecApi.GetGpoAsync(codecInformation.Ip, number) ?? false;
                return new GpoViewModel { Number = number, Active = gpoActive };
            });
        }

        [Route("SetInputEnabled")]
        [HttpPost]
        public async Task<ActionResult<InputStatusViewModel>> SetInputEnabled(string sipAddress, int input, bool enabled)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                var isEnabled = await codecApi.SetInputEnabledAsync(codecInformation.Ip, input, enabled);
                return new InputStatusViewModel { Enabled = isEnabled };
            });
        }

        [HttpPost]
        [Route("SetInputGainLevel")]
        public async Task<ActionResult<InputGainLevelViewModel>> SetInputGainLevelAsync(string sipAddress, int input, int level)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                var gainLevel = await codecApi.SetInputGainLevelAsync(codecInformation.Ip, input, level);
                return new InputGainLevelViewModel { GainLevel = gainLevel };
            });
        }

        [Route("RebootCodec")]
        [HttpPost]
        public async Task<ActionResult<bool>> RebootCodec(string sipAddress)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                return await codecApi.RebootAsync(codecInformation.Ip); 
            });
        }

        [Route("Call")]
        [HttpPost]
        public async Task<ActionResult<bool>> Call(string sipAddress, string callee, string profileName)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                return await codecApi.CallAsync(codecInformation.Ip, callee, profileName);
            });
        }


        [Route("Hangup")]
        [HttpPost]
        public async Task<ActionResult<bool>> Hangup(string sipAddress)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                return await codecApi.HangUpAsync(codecInformation.Ip);
            });

        }
        
        private async Task<ActionResult<TResult>> Execute<TResult>(string sipAddress, Func<ICodecApi, CodecInformation, Task<TResult>> f)
        {
            if (string.IsNullOrEmpty(sipAddress))
            {
                return BadRequest();
            }

            var codecInfo = _ccmService.GetCodecInformationBySipAddress(sipAddress);
            CodecInformation codecInformation = codecInfo?.CodecApiType == null ? null : codecInfo;

            var codecApiType = codecInformation?.CodecApiType;
            var codecApi  = codecApiType != null ? _serviceProvider.GetService(codecApiType) as ICodecApi : null;

            if (codecInformation == null || codecApi == null)
            {
                return CodecUnavailable();
            }

            try
            {
                return await f(codecApi, codecInformation);
            }
            catch (CodecControlException ex)
            {
                log.Warn(ex, "Exception when sending codec control command to " + sipAddress);
                return InternalServerError();
            }
            catch (Exception ex)
            {
                log.Warn(ex, "Unknown exception when sending codec control command to " + sipAddress);
                return InternalServerError();
            }
        }

    }

}
