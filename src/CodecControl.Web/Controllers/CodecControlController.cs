using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Client.Exceptions;
using CodecControl.Client.Models;
using CodecControl.Web.CCM;
using CodecControl.Web.Controllers.Base;
using CodecControl.Web.Helpers;
using CodecControl.Web.Models;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace CodecControl.Web.Controllers
{
    public class CodecControlController : ApiControllerBase
    {
        #region Constructor and members
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly IServiceProvider _serviceProvider;
        private readonly CcmService _ccmService;

        public CodecControlController(CcmService ccmService, IServiceProvider serviceProvider)
        {
            _ccmService = ccmService;
            _serviceProvider = serviceProvider;
        }
        #endregion

        [Route("isavailable")]
        [HttpGet]
        public async Task<ActionResult<bool>> IsAvailable(string sipAddress)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) => await codecApi.CheckIfAvailableAsync(codecInformation.Ip));
        }

        [Route("getavailablegpos")]
        [HttpGet]
        public async Task<ActionResult<AvailableGposResponse>> GetAvailableGpos(string sipAddress)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                string gpoNameString = codecInformation.GpoNames;
                List<string> gpoNames = (gpoNameString ?? string.Empty).Split(',').Select(s => s.Trim()).ToList();

                var model = new AvailableGposResponse();

                for (int i = 0; i < codecInformation.NrOfGpos; i++)
                {
                    bool? active = await codecApi.GetGpoAsync(codecInformation.Ip, i);

                    if (!active.HasValue)
                    {
                        // GPO missing. Expected that we passed the last GPO
                        break;
                    }

                    model.Gpos.Add(new AvailableGpo()
                    {
                        Active = active.Value,
                        Name = i < gpoNames.Count ? gpoNames[i] : $"GPO {i}",
                        Number = i
                    });
                }

                return model;
            });
        }

        [Route("getaudiostatus")]
        [HttpGet]
        public async Task<ActionResult<AudioStatusResponse>> GetAudioStatus(string sipAddress)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                var audioStatus = await codecApi.GetAudioStatusAsync(codecInformation.Ip, codecInformation.NrOfInputs, codecInformation.NrOfGpos);

                var model = new AudioStatusResponse()
                {
                    Gpos = audioStatus.Gpos,
                    InputStatus = audioStatus.InputStatus,
                    VuValues = audioStatus.VuValues
                };
                return model;
            });
        }

        //[Route("getinputgainandenabled")]
        //[HttpGet]
        //public async Task<ActionResult<InputGainAndEnabledResponse>> GetInputGainAndEnabled(string sipAddress, int input)
        //{
        //    return await Execute(sipAddress, async (codecApi, codecInformation) =>
        //    {
        //        var (enabled, gain) = await codecApi.GetInputGainAndStatusAsync(codecInformation.Ip, input);
        //        return new InputGainAndEnabledResponse { Enabled = enabled, GainLevel = gain };
        //    });
        //}

        //[Route("getinputenabled")]
        //[HttpGet]
        //public async Task<ActionResult<InputStatusResponse>> GetInputEnabled(string sipAddress, int input)
        //{
        //    return await Execute(sipAddress, async (codecApi, codecInformation) =>
        //    {
        //        var enabled = await codecApi.GetInputEnabledAsync(codecInformation.Ip, input);
        //        return new InputStatusResponse { Enabled = enabled };
        //    });
        //}

        [Route("getlinestatus")]
        [HttpGet]
        public async Task<ActionResult<LineStatusResponse>> GetLineStatus(string sipAddress)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                var model = new LineStatusResponse();
                LineStatus lineStatus = await codecApi.GetLineStatusAsync(codecInformation.Ip);

                model.LineStatus = lineStatus.StatusCode.ToString();
                model.DisconnectReasonCode = (int)lineStatus.DisconnectReason;
                model.DisconnectReasonDescription = lineStatus.DisconnectReason.Description();

                return model;
            });
        }

        //[Route("getvuvalues")]
        //[HttpGet]
        //public async Task<ActionResult<VuValuesViewModel>> GetVuValues(string sipAddress)
        //{
        //    return await Execute(sipAddress, async (codecApi, codecInformation) =>
        //    {
        //        var vuValues = await codecApi.GetVuValuesAsync(codecInformation.Ip);

        //        return new VuValuesViewModel
        //        {
        //            RxLeft = vuValues.RxLeft,
        //            RxRight = vuValues.RxRight,
        //            TxLeft = vuValues.TxLeft,
        //            TxRight = vuValues.TxRight
        //        };
        //    });
        //}

        [Route("getaudiomode")]
        [HttpGet]
        public async Task<ActionResult<AudioModeResponse>> GetAudioMode(string sipAddress)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                AudioMode result = await codecApi.GetAudioModeAsync(codecInformation.Ip);

                return new AudioModeResponse
                {
                    EncoderAudioMode = result.EncoderAudioAlgoritm,
                    DecoderAudioMode = result.DecoderAudioAlgoritm
                };
            });
        }

        [Route("setgpo")]
        [HttpPost]
        public async Task<ActionResult<GpoResponse>> SetGpo([FromBody] GpoRequestParameters request)
        {
            return await Execute(request.SipAddress, async (codecApi, codecInformation) =>
            {
                await codecApi.SetGpoAsync(codecInformation.Ip, request.Number, request.Active);
                var gpoActive = await codecApi.GetGpoAsync(codecInformation.Ip, request.Number) ?? false;
                return new GpoResponse() { Active = gpoActive };
            });
        }

        [Route("setinputenabled")]
        [HttpPost]
        public async Task<ActionResult<InputStatusResponse>> SetInputEnabled([FromBody] InputEnabledRequestParameters request)
        {
            return await Execute(request.SipAddress, async (codecApi, codecInformation) =>
            {
                var isEnabled = await codecApi.SetInputEnabledAsync(codecInformation.Ip, request.Input, request.Enabled);
                return new InputStatusResponse { Enabled = isEnabled };
            });
        }

        [HttpPost]
        [Route("setinputgain")]
        public async Task<ActionResult<InputGainLevelResponse>> SetInputGain([FromBody] InputGainRequestParameters request)
        {
            return await Execute(request.SipAddress, async (codecApi, codecInformation) =>
            {
                var gainLevel = await codecApi.SetInputGainLevelAsync(codecInformation.Ip, request.Input, request.Level);
                return new InputGainLevelResponse { GainLevel = gainLevel };
            });
        }
        
        [Route("reboot")]
        [HttpPost]
        public async Task<ActionResult<bool>> Reboot([FromBody]RequestParameters request)
        {
            return await Execute(request.SipAddress,
                async (codecApi, codecInformation) => await codecApi.RebootAsync(codecInformation.Ip));
        }

        [Route("call")]
        [HttpPost]
        public async Task<ActionResult<bool>> Call([FromBody]CallRequestParameters request)
        {
            return await Execute(request.SipAddress,
                async (codecApi, codecInformation) => await codecApi.CallAsync(codecInformation.Ip, request.Callee, request.ProfileName));
        }


        [Route("hangup")]
        [HttpPost]
        public async Task<ActionResult<bool>> Hangup([FromBody]RequestParameters request)
        {
            return await Execute(request.SipAddress, async (codecApi, codecInformation) => await codecApi.HangUpAsync(codecInformation.Ip));

        }

        private async Task<ActionResult<TResult>> Execute<TResult>(string sipAddress, Func<ICodecApi, CodecInformation, Task<TResult>> func)
        {
            using (new TimeMeasurer("CodecControl"))
            {
                try
                {
                    if (string.IsNullOrEmpty(sipAddress))
                    {
                        log.Info("Invalid request. Missing SIP address");
                        return BadRequest();
                    }

                    var codecInformation = await _ccmService.GetCodecInformationBySipAddress(sipAddress);

                    if (codecInformation == null)
                    {
                        log.Info($"Codec {sipAddress} is not currently registered in CCM.");
                        return CodecUnavailable();
                    }

                    var codecApiType = codecInformation?.CodecApiType;
                    var codecApi = codecApiType != null ? _serviceProvider.GetService(codecApiType) as ICodecApi : null;

                    if (codecApi == null || string.IsNullOrEmpty(codecInformation.Ip))
                    {
                        log.Info($"Missing information to connect to codec {sipAddress}");
                        return CodecUnavailable();
                    }

                    log.Debug($"Sending codec control command to {sipAddress} on IP {codecInformation.Ip} using API {codecInformation.Api}");
                    using (new TimeMeasurer("Make Codec Request"))
                    {
                        return await func(codecApi, codecInformation);
                    }
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


        public class RequestParameters
        {
            public string SipAddress { get; set; }
        }

        public class GpoRequestParameters : RequestParameters
        {
            public int Number { get; set; }
            public bool Active { get; set; }
        }

        public class InputEnabledRequestParameters : RequestParameters
        {
            public int Input { get; set; }
            public bool Enabled { get; set; }
        }

        public class InputGainRequestParameters : RequestParameters
        {
            public int Input { get; set; }
            public int Level { get; set; }
        }

        public class CallRequestParameters : RequestParameters
        {
            public string Callee { get; set; }
            public string ProfileName { get; set; }
        }
    }

}
