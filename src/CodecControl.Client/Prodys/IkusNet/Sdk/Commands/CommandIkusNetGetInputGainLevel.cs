using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands.Base;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Commands
{
    public class CommandIkusNetGetInputGainLevel : CommandBase
    {
        public CommandIkusNetGetInputGainLevel() : base(Command.IkusNetGetInputGainLevel, 4) {}

        public int Input { get; set; }

        protected override int EncodePayload(byte[] bytes, int offset)
        {
            return ConvertHelper.EncodeUInt((uint)Input, bytes, offset);
        }
    }

}