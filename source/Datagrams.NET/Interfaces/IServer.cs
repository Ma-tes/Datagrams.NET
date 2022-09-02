using System.Net;
using System.Net.Sockets;

namespace DatagramsNet.Interfaces
{
    public interface ISocket
    {
        public string Name { get; }
        public IPAddress IPAddress { get; }
        public Socket CurrentSocket { get; }

        public IPEndPoint EndPoint { get; }

        public Task<bool> StartServerAsync() => Task.FromResult(false);
    }
}
