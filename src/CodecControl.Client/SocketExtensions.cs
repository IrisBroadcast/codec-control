using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace CodecControl.Client
{
    public static class SocketTaskExtensions
    {
        private const int ConnectionTimedOutStatusCode = 10060;

        /// <summary>
        /// Connects the specified socket.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="endpoint">The IP endpoint.</param>
        /// <param name="timeout">The timeout in milliseconds.</param>
        public static async Task ConnectAsync(this Socket socket, EndPoint endpoint, int timeout)
        {
            TimeSpan timeOut = TimeSpan.FromMilliseconds(timeout);

            var cancellationCompletionSource = new TaskCompletionSource<bool>();
            try
            {
                using (var cts = new CancellationTokenSource(timeOut))
                {
                    var task = socket.ConnectAsync(endpoint);

                    using (cts.Token.Register(() => cancellationCompletionSource.TrySetResult(true)))
                    {
                        if (task != await Task.WhenAny(task, cancellationCompletionSource.Task))
                        {
                            throw new OperationCanceledException(cts.Token);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                socket.Close();
                throw new SocketException(ConnectionTimedOutStatusCode);
            }
        }

        public static void Connect(this Socket socket, EndPoint endpoint, int timeout)
        {
            var result = socket.BeginConnect(endpoint, null, null);

            bool success = result.AsyncWaitHandle.WaitOne(timeout, true);
            if (socket.Connected)
            {
                socket.EndConnect(result);
            }
            else
            {
                socket.Close();
                throw new SocketException(ConnectionTimedOutStatusCode);
            }
        }

    }
}
