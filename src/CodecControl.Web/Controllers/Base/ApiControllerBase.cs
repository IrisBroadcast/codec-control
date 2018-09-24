using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace CodecControl.Web.Controllers.Base
{
    //[Route("api/[controller]")]
    public class ApiControllerBase : ControllerBase
    {
        protected ActionResult CodecUnavailable()
        {
            return StatusCode((int)HttpStatusCode.ServiceUnavailable, "Codec unavailable");
        }

        protected ActionResult InternalServerError()
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError);
        }

    }
}