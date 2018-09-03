using System.Net.Sockets;
using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Responses
{
    public class IkusNetGetDecoderAudioModeResponse : IkusNetStatusResponseBase
    {
        public static IkusNetGetDecoderAudioModeResponse GetResponse(Socket socket)
        {
            var responseBytes = GetResponseBytes(socket, Command.IkusNetDecoderGetAudioMode, 4);
            return new IkusNetGetDecoderAudioModeResponse(responseBytes);
        }

        public IkusNetDspAudioAlgorithm AudioAlgorithm { get; set; }

        public IkusNetGetDecoderAudioModeResponse(byte[] responseBytes)
        {
            AudioAlgorithm = (IkusNetDspAudioAlgorithm)ConvertHelper.DecodeUInt(responseBytes, 0);
        }

    }
}