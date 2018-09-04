using System.Net.Sockets;
using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Responses
{
    public class IkusNetGetEncoderAudioModeResponse : IkusNetStatusResponseBase
    {
        public static IkusNetGetEncoderAudioModeResponse GetResponse(ProdysSocket socket)
        {
            var responseBytes = GetResponseBytes(socket, Command.IkusNetEncoderGetAudioMode, 4);
            return new IkusNetGetEncoderAudioModeResponse(responseBytes);
        }

        public IkusNetDspAudioAlgorithm AudioAlgorithm { get; set; }

        public IkusNetGetEncoderAudioModeResponse(byte[] responseBytes)
        {
            AudioAlgorithm = (IkusNetDspAudioAlgorithm)ConvertHelper.DecodeUInt(responseBytes, 0);
        }
      
    }
}