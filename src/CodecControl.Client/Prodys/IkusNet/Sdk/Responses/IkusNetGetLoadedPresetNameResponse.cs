using System.Net.Sockets;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Responses
{
    public class IkusNetGetLoadedPresetNameResponse : IkusNetStatusResponseBase
    {
        public string PresetName { get; set; }

        public IkusNetGetLoadedPresetNameResponse(SocketProxy socket)
        {
            var responseBytes = GetResponseBytes(socket, Command.IkusNetGetLoadedPresetName, 256);
            PresetName = responseBytes.ToNullTerminatedString(0, 256);
        }
        
        
    }
}