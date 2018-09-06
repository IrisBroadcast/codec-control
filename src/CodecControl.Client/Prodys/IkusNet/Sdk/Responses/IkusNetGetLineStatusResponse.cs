using System.Net.Sockets;
using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Responses
{
    public class IkusNetGetLineStatusResponse : IkusNetStatusResponseBase
    {
        public IkusNetGetLineStatusResponse(SocketProxy socket)
        {
            var responseBytes = GetResponseBytes(socket, Command.IkusNetGetLineStatus, 268);
            Address = responseBytes.ToNullTerminatedString(0, 256);
            LineStatus = (IkusNetLineStatus)ConvertHelper.DecodeUInt(responseBytes, 256);
            DisconnectionCode = (IkusNetStreamingDisconnectionReason)ConvertHelper.DecodeUInt(responseBytes, 260);
            IpCallType = (IkusNetIPCallType)ConvertHelper.DecodeUInt(responseBytes, 264);

        }

        public string Address { get; }
        public IkusNetLineStatus LineStatus { get; }
        public IkusNetStreamingDisconnectionReason DisconnectionCode { get; }
        public IkusNetIPCallType IpCallType { get; }

    }
}