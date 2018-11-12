using Microsoft.AspNetCore.Mvc;
using NLog;
using System.Linq;
using CodecControl.Web.Helpers;
using System.Threading.Tasks;

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
            var list = string.Join(", ", LogLevel.AllLoggingLevels.ToList().Select(l => $"\'{l.Name}\'").ToList());
            ViewData["LogLevels"] = list;
            ViewData["CurrentLogLevel"] = LogLevelManager.GetCurrentLevel().Name;

            return View();
        }


        [HttpPost]
        [Route("setloglevel")]
        public ActionResult<string> SetLogLevel([FromBody]SetLogLevelRequest request)
        {
            LogLevelManager.SetLogLevel(request.LogLevel);
            var newLevel = LogLevelManager.GetCurrentLevel().Name;
            return newLevel;
        }

        public class SetLogLevelRequest
        {
            public string LogLevel { get; set; }
        }
    }
}