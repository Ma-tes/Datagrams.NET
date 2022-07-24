using System.Net;
using System.Net.Sockets;

namespace DatagramsNet.Interfaces
{
    public interface IServer
    {
        public string Name { get; }

        public IPAddress IPAddress { get; }

        public Socket ServerSocket { get; }

        public List<Client> Clients { get; }

        public IPEndPoint EndPoint { get; }

        public Task<bool> StartServerAsync() => Task.FromResult(false);

        public int GetRecievingDataLength() => ServerSocket.Available;
    }
}
