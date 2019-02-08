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
using System.Threading.Tasks;
using CodecControl.Web.CCM;
using CodecControl.Web.Controllers.Base;
using CodecControl.Web.Helpers;
using CodecControl.Web.Models.Requests;
using CodecControl.Web.Security;
using Microsoft.AspNetCore.Mvc;

namespace CodecControl.Web.Controllers.CodecControl
{
    [BasicAuthorize]
    [Route("api/codeccontrol")]
    public class CallController : CodecControlControllerBase
    {
        public CallController(CcmService ccmService, IServiceProvider serviceProvider) : base(ccmService, serviceProvider)
        {
        }

        [Route("call")]
        [HttpPost]
        public async Task<ActionResult<bool>> Call([FromBody]CallRequest request)
        {
            if (request == null) { return BadRequest(); }

            log.Info($"Request to Call. SipAddress={request.SipAddress} Callee={request.Callee} Profile={request.ProfileName} WhichCodec={request.DeviceEncoder}");

            var caller = new SipUri(request.SipAddress).UserAtHost;

            string callee = request.Callee; // Kan vara antingen sip-adress eller telefonnr (som saknar domän).
            if (!callee.IsNumeric())
            {
                // Sip-adress.
                callee = new SipUri(callee).UserAtHost;
            }

            string deviceEncoder = request.DeviceEncoder ?? "Program";

            return await Execute(caller,
                async (codecApi, codecInformation) => await codecApi.CallAsync(codecInformation.Ip, callee, request.ProfileName, deviceEncoder));
        }
        
        [Route("hangup")]
        [HttpPost]
        public async Task<ActionResult<bool>> Hangup([FromBody]HangupRequest request)
        {
            if (request == null) { return BadRequest(); }

            log.Info($"Request to Hangup. SipAddress={request.SipAddress}");

            var caller = new SipUri(request.SipAddress).UserAtHost;
            return await Execute(caller, async (codecApi, codecInformation) => await codecApi.HangUpAsync(codecInformation.Ip));
        }

        [Route("reboot")]
        [HttpPost]
        public async Task<ActionResult<bool>> Reboot([FromBody]RebootRequest request)
        {
            if (request == null) { return BadRequest(); }
            return await Execute(request.SipAddress,
                async (codecApi, codecInformation) => await codecApi.RebootAsync(codecInformation.Ip));
        }
    }
}