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
            ViewData["Server"] = _appSettings.Server;
            ViewData["Version"] = "v" + _appSettings.Version;
            ViewData["ReleaseDate"] = _appSettings.ReleaseDate;

            return View();
        }

        [Route("help")]
        public IActionResult Help()
        {
            ViewData["Server"] = _appSettings.Server;
            ViewData["Version"] = "v"+_appSettings.Version;
            ViewData["ReleaseDate"] = _appSettings.ReleaseDate;

            return View();
        }
    }

}