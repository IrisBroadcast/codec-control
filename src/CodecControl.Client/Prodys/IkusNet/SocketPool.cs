using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace CodecControl.Client.Prodys.IkusNet
{
    
    /// <summary>
    /// Håller dictionary med uppkopplade sockets där ip-adress är nyckel
    /// </summary>
    public class SocketPool
    {
        private readonly ConcurrentDictionary<string, ConcurrentBag<ProdysSocket>> _dictionary;

        public SocketPool()
        {
            _dictionary = new ConcurrentDictionary<string, ConcurrentBag<ProdysSocket>>();
            
            // TODO: skapa timer som var 10:e sekund plockar bort gamla sockets från poolen
        }

        public async Task<ProdysSocket> GetSocket(string ipAddress)
        {
            var dictionaryForIpAddress = _dictionary.GetOrAdd(ipAddress, s => new ConcurrentBag<ProdysSocket>());
            
            if (!dictionaryForIpAddress.TryTake(out var socket))
            {
                socket = await ProdysSocket.GetConnectedSocketAsync(ipAddress);
            }

            return socket;
        }

        public void ReturnSocket(ProdysSocket socket)
        {
            var dictionaryForIpAddress = _dictionary.GetOrAdd(socket.IpAddress, s => new ConcurrentBag<ProdysSocket>());
            socket.UpdateEvictionTime();
            dictionaryForIpAddress.Add(socket);
        }

        private void EvictOldSockets()
        {
            foreach (var dictionaryForIpAddress in _dictionary.Values)
            {
                var socketsToEvict = dictionaryForIpAddress.Where(s => s.IsOld());
                foreach (var prodysSocket in socketsToEvict)
                {
                    // TODO: Plocka bort socketen ur listan. Svårt att göra med ConcurrentBag dock.

                    prodysSocket.Close();
                    prodysSocket.Dispose();
                }
            }
        }

    }
}