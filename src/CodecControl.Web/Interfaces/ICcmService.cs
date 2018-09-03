using CodecControl.Client;

namespace CodecControl.Web.Interfaces
{
    public interface ICcmService
    {
        CodecInformation GetCodecInformationBySipAddress(string sipAddress);
    }
}