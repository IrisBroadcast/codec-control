using System.Collections.Generic;
using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Web.CCM;
using CodecControl.Web.Hub;
using Microsoft.AspNetCore.Mvc;

namespace CodecControl.Web.Controllers
{
    [Route("debug")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DebugController : ControllerBase
    {
        private readonly AudioStatusUpdater _audioStatusUpdater;
        private readonly CcmService _ccmService;

        public DebugController(AudioStatusUpdater audioStatusUpdater, CcmService ccmService)
        {
            _audioStatusUpdater = audioStatusUpdater;
            _ccmService = ccmService;
        }
        
        [HttpGet]
        [Route("subscriptions")]
        public List<SubscriptionInfo> Subscriptions()
        {
            return _audioStatusUpdater.Subscriptions;
        }

        [HttpGet]
        [Route("codecinformation")]
        public async Task<CodecInformation> CodecInformation(string sipAddress)
        {
            return await _ccmService.GetCodecInformationBySipAddress(sipAddress);
        }

    }
}