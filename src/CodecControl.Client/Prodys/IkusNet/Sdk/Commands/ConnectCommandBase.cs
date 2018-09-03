using CodecControl.Client.Prodys.IkusNet.Sdk.Commands.Base;
using CodecControl.Client.Prodys.IkusNet.Sdk.Enums;

namespace CodecControl.Client.Prodys.IkusNet.Sdk.Commands
{
    public abstract class ConnectCommandBase : ICommandBase
    {
        public abstract Command Command { get; }
        public abstract byte[] GetBytes();
    }
}