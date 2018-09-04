using System;
using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Responses
{
    public class AcknowledgeResponse : ResponseHeader
    {
        public Command ReceivedCommand { get; set; }
        public bool Acknowleged { get; set; }

        public AcknowledgeResponse(ProdysSocket socket)
        {
            var buffer = new byte[16];
            socket.Receive(buffer);

            Command = (Command)ConvertHelper.DecodeUInt(buffer, 0);
            Length = (int)ConvertHelper.DecodeUInt(buffer, 4);
            ReceivedCommand = (Command)ConvertHelper.DecodeUInt(buffer, 8);
            Acknowleged = Convert.ToBoolean(ConvertHelper.DecodeUInt(buffer, 12));

        }
        
        public override string ToString()
        {
            return $"{base.ToString()}, Received Command={ReceivedCommand}, Acknowleged={Acknowleged} ";
        }
    }
}