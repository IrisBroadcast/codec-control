using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands.Base;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Commands
{
    public class CommandIkusNetGetEncoderAudioMode : CommandBase
    {
        public CommandIkusNetGetEncoderAudioMode() : base(Command.IkusNetEncoderGetAudioMode, 4) {}

        protected override int EncodePayload(byte[] bytes, int offset)
        {
            return ConvertHelper.EncodeUInt((uint)IkusNetCodec.Program, bytes, offset);
        }
    }

}