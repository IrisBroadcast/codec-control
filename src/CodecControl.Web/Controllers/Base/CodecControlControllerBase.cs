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
using CodecControl.Client;
using CodecControl.Client.Exceptions;
using CodecControl.Web.CCM;
using CodecControl.Web.Models.Requests;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace CodecControl.Web.Controllers.Base
{
    public class CodecControlControllerBase : ApiControllerBase
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected readonly IServiceProvider _serviceProvider;
        protected readonly CcmService _ccmService;

        public CodecControlControllerBase(CcmService ccmService, IServiceProvider serviceProvider)
        {
            _ccmService = ccmService;
            _serviceProvider = serviceProvider;
        }

        protected async Task<ActionResult<TResult>> Execute<TResult>(string sipAddress, Func<ICodecApi, CodecInformation, Task<TResult>> func)
        {
            using (new TimeMeasurer("CodecControl"))
            {
                try
                {

                    if (string.IsNullOrEmpty(sipAddress))
                    {
                        log.Info($"Invalid request. Missing SIP address in request: {Request.GetDisplayUrl()} " +
                                 $"[Host={Request.Headers["Host"]}, UserAgent={Request.Headers["User-Agent"]}]");
                        return BadRequest();
                    }

                    var codecInformation = await _ccmService.GetCodecInformationBySipAddress(sipAddress);

                    if (codecInformation == null)
                    {
                        log.Info($"Codec {sipAddress} is not currently registered in CCM.");
                        return CodecUnavailable();
                    }

                    var codecApiType = codecInformation.CodecApiType;
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
                catch (CodecInvocationException ex)
                {
                    // When response from codec was unparsable or indicates that request was unsuccessful.
                    log.Info($"Error when sending codec control command to {sipAddress}. {ex.Message}");
                    return InternalServerError();
                }
                catch (CodecException ex)
                {
                    log.Warn(ex, "Exception when sending codec control command to " + sipAddress);
                    return InternalServerError();
                }
                catch (Exception ex)
                {
                    log.Warn(ex, "Exception when sending codec control command to " + sipAddress);
                    return InternalServerError();
                }
            }
        }

    }
}