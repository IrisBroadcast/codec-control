using System.Collections.Generic;
using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Web.CCM;
using CodecControl.Web.HostedServices;
using CodecControl.Web.Hub;
using Microsoft.AspNetCore.Mvc;

namespace CodecControl.Web.Controllers
{
    [Route("debug")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DebugController : ControllerBase
    {
        private readonly CcmService _ccmService;
        private readonly AudioStatusService _audioStatusService;

        public DebugController(CcmService ccmService, AudioStatusService audioStatusService)
        {
            _ccmService = ccmService;
            _audioStatusService = audioStatusService;
        }
        
        [HttpGet]
        [Route("subscriptions")]
        public List<SubscriptionInfo> Subscriptions()
        {
            return _audioStatusService.Subscriptions;
        }

        [HttpGet]
        [Route("codecinformation")]
        public async Task<CodecInformation> CodecInformation(string sipAddress)
        {
            return await _ccmService.GetCodecInformationBySipAddress(sipAddress);
        }

    }
}