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

    public abstract partial class SocketManager : ISocket
    {
        public string Name { get; }
        public IPAddress IPAddress { get; }

        public virtual Socket CurrentSocket { get; set; }
    }

    public abstract partial class SocketManager : IReciever
    {
        public virtual int PortNumber => 1111;
        public virtual Func<bool> CancellationFunction { get; set; } = () => false;

        public IPEndPoint EndPoint { get; set; }

        public SocketReciever SocketReciever { get; set; }

        protected abstract int bufferSize { get; set; }

        public Socket RecieveSocketHandler { get; protected set; }

        public SocketManager(IPAddress ipAddress) 
        {
            Name = this.GetType().Name;
            IPAddress = ipAddress;
            EndPoint = new IPEndPoint(IPAddress, PortNumber);
        }

        public virtual Task OnRecieveAsync(object datagram, EndPoint ipAddress)
        {
            return Task.CompletedTask;
        }

        protected virtual async Task<ClientDatagram> StartRecievingAsync()
        {
            return await SocketReciever.GetDatagramDataAsync(RecieveSocketHandler);
        }

        public async Task<bool> StartServerAsync()
        {
            if (CurrentSocket.ProtocolType == ProtocolType.Tcp)
            {
                CurrentSocket.Listen();
                RecieveSocketHandler = Task.Run(() => CurrentSocket.AcceptAsync()).Result;
            }
            else
                RecieveSocketHandler = CurrentSocket;

            await SocketReciever.StartRecievingAsync(OnRecieveAsync, StartRecievingAsync, CancellationFunction);
            return false;
        }
    }
}
