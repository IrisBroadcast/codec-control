using System.Threading.Tasks;
using CodecControl.Client;

namespace CodecControl.Web.Interfaces
{
    public interface ICcmService
    {
        Task<CodecInformation> GetCodecInformationBySipAddress(string sipAddress);
    }
}