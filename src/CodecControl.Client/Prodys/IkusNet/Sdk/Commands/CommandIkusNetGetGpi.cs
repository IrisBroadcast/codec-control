using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands.Base;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Commands
{
    public class CommandIkusNetGetGpi : CommandBase
    {
        public CommandIkusNetGetGpi() : base(Command.IkusNetGetGpi, 4)
        {
        }

        public int Gpio { get; set; }

        protected override int EncodePayload(byte[] bytes, int offset)
        {
            return ConvertHelper.EncodeUInt((uint)Gpio, bytes, offset);
        }
    }
}