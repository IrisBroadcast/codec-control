using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands.Base;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Commands
{
    public class CommandIkusNetSetInputGainLevel : CommandBase
    {
        public CommandIkusNetSetInputGainLevel() : base(Command.IkusNetSetInputGainLevel, 8) {}

        public int Input { get; set; }
        public int GainLeveldB { get; set; }

        protected override int EncodePayload(byte[] bytes, int offset)
        {
            offset = ConvertHelper.EncodeUInt((uint)Input, bytes, offset);
            offset = ConvertHelper.EncodeUInt((uint)GainLeveldB, bytes, offset);
            return offset;
        }
        
    }
}