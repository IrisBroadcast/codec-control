using System.Collections.Generic;
using System.Threading.Tasks;
using CodecControl.Client;

namespace CodecControl.Web.Services
{
    public interface ICcmRepository
    {
        Task<List<CodecInformation>> GetCodecInformationList();
    }
}