using System;
using System.Threading.Tasks;
using CodecControl.Web.CCM;
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
            var caller = new SipUri(request.SipAddress).UserAtHost;

            string callee = request.Callee; // Kan vara antingen sip-adress eller telefonnr (som saknar domän).
            if (!callee.IsNumeric())
            {
                // Sip-adress.
                callee = new SipUri(callee).UserAtHost;
            }

            string whichCodec = request.WhichCodec ?? "Program";

            return await Execute(caller,
                async (codecApi, codecInformation) => await codecApi.CallAsync(codecInformation.Ip, callee, request.ProfileName, whichCodec));
        }
        
        [Route("hangup")]
        [HttpPost]
        public async Task<ActionResult<bool>> Hangup([FromBody]HangupRequest request)
        {
            var caller = new SipUri(request.SipAddress).UserAtHost;
            return await Execute(caller, async (codecApi, codecInformation) => await codecApi.HangUpAsync(codecInformation.Ip));
        }

        [Route("reboot")]
        [HttpPost]
        public async Task<ActionResult<bool>> Reboot([FromBody]RebootRequest request)
        {
            return await Execute(request.SipAddress,
                async (codecApi, codecInformation) => await codecApi.RebootAsync(codecInformation.Ip));
        }
    }
}