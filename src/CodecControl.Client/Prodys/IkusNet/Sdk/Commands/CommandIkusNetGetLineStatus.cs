using CodecControl.Client.Prodys.Helpers;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands.Base;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Commands
{
    public class CommandIkusNetGetLineStatus : CommandBase
    {
        public CommandIkusNetGetLineStatus() : base(Command.IkusNetGetLineStatus, 4)
        {
        }
        public IkusNetLine Line { get; set; }
        protected override int EncodePayload(byte[] bytes, int offset)
        {
            return ConvertHelper.EncodeUInt((uint)Line, bytes, offset);
        }
    }
    
}