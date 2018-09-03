using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands.Base;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Commands
{
    public class CommandIkusNetSysSetDeviceName : CommandBase
    {
        public CommandIkusNetSysSetDeviceName() : base(Command.IkusNetSysSetDeviceName, 256)
        {
        }

        public string DeviceName { get; set; }

        protected override int EncodePayload(byte[] bytes, int offset)
        {
            offset = ConvertHelper.EncodeString(DeviceName, bytes, offset, 256);
            return offset;
        }
    }

}