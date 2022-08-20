using DatagramsNet.Interfaces;
using System.Net;
using System.Net.Sockets;

namespace DatagramsNet
{
    public readonly struct ClientDatagram
    {
        public IPEndPoint Client { get; init; }
        public byte[] Datagram { get; init; } // TODO: Use Memory<T> instead
    }

    public abstract partial class ServerManager : IServer
    {
        public string Name { get; }
        public IPAddress IPAddress { get; }
        public Socket ServerSocket { get; }
        public List<Client> Clients { get; } = new();

        public static ServerManager CreateUdpServer(Socket serverSocket, IPAddress ipAddress, Func<object, EndPoint, ValueTask> receiveFunc)
        {
            return new UdpServer(serverSocket, ipAddress, receiveFunc);
        }

        private sealed class UdpServer : ServerManager
        {
            private readonly Func<object, EndPoint, ValueTask> receive;

            public UdpServer(Socket serverSocket, IPAddress ipAddress, Func<object, EndPoint, ValueTask> receive) : base(serverSocket, ipAddress)
            {
                this.receive = receive;
            }

            public override async Task OnRecieveAsync(object datagram, EndPoint ipAddress)
            {
                await receive(datagram, ipAddress);
            }
        }
    }

    public abstract partial class ServerManager : IReciever
    {
        public virtual int PortNumber => 1111;
        public UdpReciever UdpReciever { get; set; }
        public IPEndPoint EndPoint { get; }

        public ServerManager(Socket serverSocket, IPAddress ipAddress)
        {
            Name = $"Server {Guid.NewGuid()}";
            ServerSocket = serverSocket;
            IPAddress = ipAddress;
            EndPoint = new IPEndPoint(ipAddress, PortNumber);
            UdpReciever = new UdpReciever(serverSocket);
        }

        public ServerManager(string name, IPAddress ipAddress)
        {
            Name = name;
            IPAddress = ipAddress;
            EndPoint = new IPEndPoint(IPAddress, PortNumber);

            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ServerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            ServerSocket.Bind(EndPoint);
            ServerSocket.Blocking = false;
            UdpReciever = new UdpReciever(ServerSocket);
        }

        public virtual Task OnRecieveAsync(object datagram, EndPoint ipAddress)
        {
            return Task.CompletedTask;
        }

        protected virtual async Task<ClientDatagram> StartRecievingAsync()
        {
            return await UdpReciever.GetDatagramDataAsync();
        }

        public async Task<bool> StartServerAsync()
        {
            await UdpReciever.StartRecievingAsync(OnRecieveAsync, StartRecievingAsync);
            return false;
        }
    }
}
