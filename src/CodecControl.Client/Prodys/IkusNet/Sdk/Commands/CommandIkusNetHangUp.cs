using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands.Base;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Commands
{
    public sealed class CommandIkusNetHangUp : CommandBase
    {
        public CommandIkusNetHangUp() : base (Command.IkusNetHangUp, 4)
        {
        }

        public IkusNetCodec Codec { get; set; }

        protected override int EncodePayload(byte[] bytes, int offset)
        {
            return ConvertHelper.EncodeUInt((uint)Codec, bytes, offset);
        }
    }
}