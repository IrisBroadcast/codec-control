using CodecControl.Web.Models;

namespace CodecControl.Web.Services
{
    public interface ICcmService
    {
        CodecInformation GetCodecInformationBySipAddress(string sipAddress);
    }
}