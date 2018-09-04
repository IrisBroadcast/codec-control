using System;
using System.Net.Sockets;
using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Responses
{
    public class IkusNetGetInputEnabledResponse : IkusNetStatusResponseBase
    {
        public IkusNetGetInputEnabledResponse(ProdysSocket socket)
        {
            var response = GetResponseBytes(socket, Command.IkusNetGetInputEnabled, 4);
            Enabled = Convert.ToBoolean(ConvertHelper.DecodeUInt(response, 0));
        }

        public bool Enabled { get; set; }

    }
}