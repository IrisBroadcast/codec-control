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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodecControl.Client.Models;
using CodecControl.Web.CCM;
using CodecControl.Web.Controllers.Base;
using CodecControl.Web.Helpers;
using CodecControl.Web.Models.Requests;
using CodecControl.Web.Models.Responses;
using CodecControl.Web.Security;
using Microsoft.AspNetCore.Mvc;

namespace CodecControl.Web.Controllers.CodecControl
{
    [Route("api/codeccontrol")]
    public class CodecControlController : CodecControlControllerBase
    {
        public CodecControlController(CcmService ccmService, IServiceProvider serviceProvider) : base(ccmService, serviceProvider)
        {
        }

        [Route("isavailable")]
        [HttpGet]
        public async Task<ActionResult<IsAvailableResponse>> IsAvailable(string sipAddress)
        {
            
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                var isAvailable = await codecApi.CheckIfAvailableAsync(codecInformation.Ip);
                return new IsAvailableResponse()
                {
                    IsAvailable = isAvailable
                };
            });
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

        [Route("getinputgainandenabled")]
        [HttpGet]
        public async Task<ActionResult<InputGainAndEnabledResponse>> GetInputGainAndEnabled(string sipAddress, int input)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                var (enabled, gain) = await codecApi.GetInputGainAndStatusAsync(codecInformation.Ip, input);
                return new InputGainAndEnabledResponse
                {
                    Enabled = enabled,
                    GainLevel = gain
                };
            });
        }

        [Route("getinputenabled")]
        [HttpGet]
        public async Task<ActionResult<InputEnabledResponse>> GetInputEnabled(string sipAddress, int input)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                var enabled = await codecApi.GetInputEnabledAsync(codecInformation.Ip, input);
                return new InputEnabledResponse
                { 
                    Enabled = enabled
                };
            });
        }

        [Route("getlinestatus")]
        [HttpGet]
        public async Task<ActionResult<LineStatusResponse>> GetLineStatus(string sipAddress, string deviceEncoder = "ProgramL1")
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                var model = new LineStatusResponse();

                string deviceLineEncoder = deviceEncoder ?? "ProgramL1";

                LineStatus lineStatus = await codecApi.GetLineStatusAsync(codecInformation.Ip, deviceLineEncoder);

                model.LineStatus = lineStatus.StatusCode.ToString();
                model.DisconnectReasonCode = (int)lineStatus.DisconnectReason;
                model.DisconnectReasonDescription = lineStatus.DisconnectReason.Description();
                model.RemoteAddress = lineStatus.RemoteAddress;

                return model;
            });
        }

        [Route("getvuvalues")]
        [HttpGet]
        public async Task<ActionResult<VuValuesResponse>> GetVuValues(string sipAddress)
        {
            return await Execute(sipAddress, async (codecApi, codecInformation) =>
            {
                var vuValues = await codecApi.GetVuValuesAsync(codecInformation.Ip);

                return new VuValuesResponse
                {
                    RxLeft = vuValues.RxLeft,
                    RxRight = vuValues.RxRight,
                    TxLeft = vuValues.TxLeft,
                    TxRight = vuValues.TxRight
                };
            });
        }

        /// <summary>
        /// Vu-values är en dB-skala, där 0 = full scale.
        /// Exempelvis motsvarar 0 dB vid 18dB full scale ett värde på -18.
        /// </summary>
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

        [BasicAuthorize]
        [Route("setgpo")]
        [HttpPost]
        public async Task<ActionResult<GpoResponse>> SetGpo([FromBody] SetGpoRequest request)
        {
            if (request == null) { return BadRequest(); }

            return await Execute(request.SipAddress, async (codecApi, codecInformation) =>
            {
                await codecApi.SetGpoAsync(codecInformation.Ip, request.Number, request.Active);
                var gpoActive = await codecApi.GetGpoAsync(codecInformation.Ip, request.Number) ?? false;
                return new GpoResponse()
                {
                    Active = gpoActive
                };
            });
        }

        [BasicAuthorize]
        [Route("setinputenabled")]
        [HttpPost]
        public async Task<ActionResult<InputEnabledResponse>> SetInputEnabled([FromBody] SetInputEnabledRequest request)
        {
            if (request == null) { return BadRequest(); }

            return await Execute(request.SipAddress, async (codecApi, codecInformation) =>
            {
                var isEnabled = await codecApi.SetInputEnabledAsync(codecInformation.Ip, request.Input, request.Enabled);
                return new InputEnabledResponse
                {
                    Enabled = isEnabled
                };
            });
        }

        [BasicAuthorize]
        [HttpPost]
        [Route("increaseinputgain")]
        public async Task<ActionResult<InputGainLevelResponse>> IncreaseInputGain([FromBody]ChangeGainRequest parameters)
        {
            return await ChangeInputGain(parameters, 1);
        }

        [BasicAuthorize]
        [HttpPost]
        [Route("decreaseinputgain")]
        public async Task<ActionResult<InputGainLevelResponse>> DecreaseInputGain([FromBody]ChangeGainRequest parameters)
        {
            return await ChangeInputGain(parameters, -1);
        }

        [BasicAuthorize]
        [HttpPost]
        [Route("changeinputgain")]
        public async Task<ActionResult<InputGainLevelResponse>> ChangeInputGain(ChangeGainRequest request, int change)
        {
            if (request == null) { return BadRequest(); }

            return await Execute(request.SipAddress, async (codecApi, codecInformation) =>
            {
                var gain = await codecApi.GetInputGainLevelAsync(codecInformation.Ip, request.Input);
                var newGain = gain + change;
                var setGain = await codecApi.SetInputGainLevelAsync(codecInformation.Ip, request.Input, newGain);
                return new InputGainLevelResponse
                {
                    GainLevel = setGain
                };
            });
        }

        [BasicAuthorize]
        [HttpPost]
        [Route("setinputgain")]
        public async Task<ActionResult<InputGainLevelResponse>> SetInputGain([FromBody] SetInputGainRequest request)
        {
            if (request == null) { return BadRequest(); }

            return await Execute(request.SipAddress, async (codecApi, codecInformation) =>
            {
                var gainLevel = await codecApi.SetInputGainLevelAsync(codecInformation.Ip, request.Input, request.Level);
                return new InputGainLevelResponse
                {
                    GainLevel = gainLevel
                };
            });
        }

        [BasicAuthorize]
        [HttpPost]
        [Route("batchsetinputenabled")]
        public async Task<ActionResult<BatchEnableInputsResponse>> BatchSetInputEnabled([FromBody] BatchInputEnableRequest request)
        {
            if (request == null) { return BadRequest(); }

            return await Execute(request.SipAddress, async (codecApi, codecInformation) =>
            {
                var result = new BatchEnableInputsResponse();

                foreach (var command in request.InputEnableRequests)
                {
                    var enabled = await codecApi.SetInputEnabledAsync(codecInformation.Ip, command.Input, command.Enabled);
                    result.Inputs.Add(new InputEnabledStatus{ Enabled = enabled });
                }

                return result;
            });
        }
    }
}
