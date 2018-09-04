using System.Net.Sockets;
using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Responses
{
    public class IkusNetGetInputGainLevelResponse : IkusNetStatusResponseBase
    {
        public int GainLeveldB { get; set; }

        public IkusNetGetInputGainLevelResponse(ProdysSocket socket)
        {
            var responseBytes = GetResponseBytes(socket, Command.IkusNetGetInputGainLevel, 4);
            GainLeveldB = (int)ConvertHelper.DecodeUInt(responseBytes, 0);
        }

        
    }

   

}