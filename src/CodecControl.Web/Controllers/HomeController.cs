using Microsoft.AspNetCore.Mvc;

namespace CodecControl.Web.Controllers
{
    [Route("")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        private readonly ApplicationSettings _appSettings;

        public HomeController(ApplicationSettings appSettings)
        {
            _appSettings = appSettings;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            ViewData["Server"] = _appSettings.Server;
            ViewData["Version"] = "v" + _appSettings.Version;
            ViewData["ReleaseDate"] = _appSettings.ReleaseDate;

            return View();
        }

    }

}