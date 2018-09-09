using CodecControl.Web.Hubs;
using Microsoft.AspNetCore.Mvc;

namespace CodecControl.Web.Controllers
{
    [Route("debug")]
    public class DebugController : ControllerBase
    {
        private readonly AudioStatusUpdater _audioStatusUpdater;

        public DebugController(AudioStatusUpdater audioStatusUpdater)
        {
            _audioStatusUpdater = audioStatusUpdater;
        }

        public ActionResult<SubscriptionInfo> Get()
        {
            return Ok(_audioStatusUpdater.Subscriptions);
        }
    }
}