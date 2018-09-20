using System.Collections.Generic;
using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Web.AudioStatus;
using CodecControl.Web.CCM;
using Microsoft.AspNetCore.Mvc;

namespace CodecControl.Web.Controllers
{
    [Route("debug")]
    public class DebugController : ControllerBase
    {
        private readonly AudioStatusUpdater _audioStatusUpdater;
        private readonly CcmService _ccmService;

        public DebugController(AudioStatusUpdater audioStatusUpdater, CcmService ccmService)
        {
            _audioStatusUpdater = audioStatusUpdater;
            _ccmService = ccmService;
        }
        
        [Route("subscriptions")]
        public List<SubscriptionInfo> Subscriptions()
        {
            return _audioStatusUpdater.Subscriptions;
        }

        [Route("codecinformation")]
        public async Task<List<CodecInformation>> CodecInformation()
        {
            List<CodecInformation> codecInformations = await _ccmService.GetCodecInformationList();
            return codecInformations;
        }

    }
}