using System;
using Microsoft.AspNetCore.Mvc;

namespace CodecControl.Web.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly ApplicationSettings _appSettings;

        public HomeController(ApplicationSettings appSettings)
        {
            _appSettings = appSettings;
        }

        [Route("")]
        public IActionResult Index()
        {
            ViewData["Version"] = _appSettings.Version;
            ViewData["ReleaseDate"] = _appSettings.ReleaseDate;

            return View();
        }


    }
}