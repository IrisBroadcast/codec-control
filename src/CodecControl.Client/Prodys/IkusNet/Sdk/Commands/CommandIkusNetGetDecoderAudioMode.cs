using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands.Base;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Commands
{
    public class CommandIkusNetGetDecoderAudioMode : CommandBase
    {
        public CommandIkusNetGetDecoderAudioMode() : base(Command.IkusNetDecoderGetAudioMode, 4)
        {
        }

        protected override int EncodePayload(byte[] bytes, int offset)
        {
            return ConvertHelper.EncodeUInt((uint)IkusNetCodec.Program, bytes, offset);
        }
    }
}