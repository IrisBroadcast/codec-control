using System;
using CodecControl.Web.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace CodecControl.Web.Controllers
{
    [Route("")]
    public class HomeController : ApiControllerBase
    {
        [HttpGet]
        [Route("")]
        public string Get()
        {
            return $"Codec Control. {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }
    }
}