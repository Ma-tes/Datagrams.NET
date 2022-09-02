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

        public IPEndPoint EndPoint { get; }

        public SocketReciever SocketReciever { get; set; }

        protected abstract int bufferSize { get; set; }

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
            return await SocketReciever.GetDatagramDataAsync();
        }

        public async Task<bool> StartServerAsync()
        {
            await SocketReciever.StartRecievingAsync(OnRecieveAsync, StartRecievingAsync, CancellationFunction);
            return false;
        }
    }
}
