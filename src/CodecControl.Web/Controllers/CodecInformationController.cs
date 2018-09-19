using System.Collections.Generic;
using System.Threading.Tasks;
using CodecControl.Client;
using CodecControl.Web.CCM;
using Microsoft.AspNetCore.Mvc;

namespace CodecControl.Web.Controllers
{
    [Route("codecinformation")]
    public class CodecInformationController : ControllerBase
    {
        private readonly CcmService _ccmService;

        public CodecInformationController(CcmService ccmService)
        {
            _ccmService = ccmService;
        }
        public async Task<List<CodecInformation>> Get()
        {
            List<CodecInformation> codecInformations = await _ccmService.GetCodecInformationList();
            return codecInformations;
        }

    }
}