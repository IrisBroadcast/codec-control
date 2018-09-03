using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands.Base;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Commands
{
    public class CommandIkusNetGetLoadedPresetName : CommandBase
    {
        public CommandIkusNetGetLoadedPresetName() : base(Command.IkusNetGetLoadedPresetName, 256)
        {
        }

        public string LastLoadedPresetName { get; set; }

        protected override int EncodePayload(byte[] bytes, int offset)
        {
            return ConvertHelper.EncodeString(LastLoadedPresetName, bytes, offset, 256);
        }

    }

}