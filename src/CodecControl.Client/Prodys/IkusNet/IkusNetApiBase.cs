using System.Threading.Tasks;
using CodecControl.Client.Prodys.IkusNet.Sdk.Commands.Base;
using CodecControl.Client.Prodys.IkusNet.Sdk.Responses;
using NLog;

namespace CodecControl.Client.Prodys.IkusNet
{
    public abstract class IkusNetApiBase
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();
        protected readonly SocketPool SocketPool;

        protected IkusNetApiBase(SocketPool socketPool)
        {
            SocketPool = socketPool;
        }

        protected async Task<bool> SendConfigurationCommandAsync(string hostAddress, ICommandBase cmd)
        {
            using (var socket = await SocketPool.TakeSocket(hostAddress))
            {
                SendCommand(socket, cmd);
                var ackResponse = new AcknowledgeResponse(socket);
                return ackResponse.Acknowleged;
            }
        }

        protected int SendCommand(SocketProxy socket, ICommandBase command)
        {
            return socket.Send(command.GetBytes());
        }
    }
}