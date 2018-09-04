using System.Net.Sockets;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Responses
{
    public class IkusNetGetDeviceNameResponse : IkusNetStatusResponseBase
    {
        public string DeviceName { get; private set; }

        public IkusNetGetDeviceNameResponse(ProdysSocket socket)
        {
            var payload = GetResponseBytes(socket, Command.IkusNetSysGetDeviceName, 256);
            DeviceName = payload.ToNullTerminatedString(0, 256);
        }

    }
}