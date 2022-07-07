using System.Net;
using System.Net.Sockets;
using DatagramsNet.Interfaces;

namespace DatagramsNet
{
    public struct ClientDatagram 
    {
        public IPEndPoint Client { get; set; }

        public byte[] Datagram { get; set; } //TODO: Memory<T>
    }

    public abstract partial class ServerManager : IServer 
    {
        public string Name { get; set; }

        public IPAddress IPAddress { get; set; }

        public Socket serverSocket { get; set; }

        public List<Client> Clients { get; set; }
    }

    public abstract partial class ServerManager : IReciever
    {
        public virtual int PortNumber => 1111;

        public IPEndPoint EndPoint => new IPEndPoint(IPAddress, PortNumber);

        public UdpReciever UdpReciever { get; set; }

        public virtual async Task OnRecieveAsync(object datagram, EndPoint ipAddress) { }

        protected virtual async Task<ClientDatagram> StartRecieving() { return await UdpReciever.GetDatagramDataAsync(); }

        public ServerManager(Socket serverSocket) => UdpReciever = new UdpReciever(serverSocket);

        public ServerManager(string name, IPAddress ipAddress) 
        {
            Name = name;
            IPAddress = ipAddress;

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            serverSocket.Bind(EndPoint);
            serverSocket.Blocking = false;
            UdpReciever = new UdpReciever(serverSocket);
        }

        public async Task<bool> StartServer() 
        {
            await UdpReciever.StartRecievingAsync(OnRecieveAsync, StartRecieving);
            return await Task.FromResult(false);
        }
    }
}
