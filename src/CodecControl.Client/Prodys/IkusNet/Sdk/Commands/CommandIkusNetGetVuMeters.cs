using CodecControl.Client.Prodys.IkusNet.Sdk.Commands.Base;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Commands
{
    public class CommandIkusNetGetVuMeters : CommandBase
    {
        public CommandIkusNetGetVuMeters() : base(Command.IkusNetGetVumeters, 0) {}

    }
}