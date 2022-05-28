using System.Net;
using System.Net.Sockets;

namespace DatagramsNet.Interfaces
{
    public interface IServer
    {
        public string Name { get; set; }

        public IPAddress IPAddress { get; set; }

        public Socket serverSocket { get; set; }

        public List<Client> Clients { get; set; }

        public IPEndPoint EndPoint { get; }

        public async Task<bool> StartServer() => false;

        public int GetRecievingDataLength() => serverSocket.Available;
    }
}
