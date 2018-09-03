using CodecControl.Web.Controllers;
using CodecControl.Web.Models;

namespace CodecControl.Web.Services
{
    public class CcmService : ICcmService
    {
        public CodecInformation GetCodecInformationBySipAddress(string sipAddress)
        {
            // TODO: Connect to CCM and retrieve codec information
            // TODO: Cache the information
            // TODO: Listen to CCM hub and reload only when necessary
            return new CodecInformation();
        }
    }
}