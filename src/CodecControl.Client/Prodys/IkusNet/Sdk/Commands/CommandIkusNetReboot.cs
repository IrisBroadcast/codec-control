using CodecControl.Client.Prodys.IkusNet.Sdk.Commands.Base;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Commands
{

    public class CommandIkusNetReboot : CommandBase
    {
        public CommandIkusNetReboot() : base(Command.IkusNetSysRebootDevice, 0)
        {
        }

    }

}