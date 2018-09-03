using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands.Base;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Commands
{
    public class CommandIkusNetGetGpo : CommandBase
    {
        public CommandIkusNetGetGpo() : base(Command.IkusNetGetGpo, 4)
        {
            
        }
        public int Gpio { get; set; }

        protected override int EncodePayload(byte[] bytes, int offset)
        {
            return ConvertHelper.EncodeUInt((uint)Gpio, bytes, offset);
        }
        
    }
}