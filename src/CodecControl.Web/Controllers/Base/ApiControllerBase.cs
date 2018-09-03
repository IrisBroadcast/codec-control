using Microsoft.AspNetCore.Mvc;

namespace CodecControl.Web.Controllers
{
    [Route("api/[controller]")]
    public class ApiControllerBase : ControllerBase
    {
        protected ActionResult CodecUnavailable()
        {
            return StatusCode(503, "Codec unavailable");
        }
    }
}