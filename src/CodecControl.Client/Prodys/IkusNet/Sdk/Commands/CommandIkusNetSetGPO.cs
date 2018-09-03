using System;
using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands.Base;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Commands
{
    public class CommandIkusNetSetGpo : CommandBase
    {
        public CommandIkusNetSetGpo() : base(Command.IkusNetSetGpo, 8)
        {
        }

        public int Gpo { get; set; }
        public bool Active { get; set; }


        protected override int EncodePayload(byte[] bytes, int offset)
        {
            offset = ConvertHelper.EncodeUInt((uint)Gpo, bytes, offset);
            offset = ConvertHelper.EncodeUInt(Convert.ToUInt32(Active), bytes, offset);
            return offset;
        }
    }
    
}