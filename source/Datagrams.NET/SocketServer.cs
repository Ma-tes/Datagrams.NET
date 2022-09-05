using DatagramsNet.Datagram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DatagramsNet
{
    public partial class SocketServer : SocketManager
    {
        //Default unknown socket inicialization, we know that's not the perfect solution, but
        //it will force another programmers to use override, instead of having it like constructor
        //parameter
        public override Socket CurrentSocket => new Socket(AddressFamily.InterNetwork, SocketType.Unknown, ProtocolType.Unknown);

        protected static readonly int DefaultBufferSize = 128000;
        protected override int bufferSize { get; set; } = DefaultBufferSize;


        public SocketServer(IPAddress address) : base(address) 
        {
        }

        public static SocketServer CreateServer(ProtocolType protocol, Func<object, EndPoint, Task> recieveFunction, IPEndPoint endPoint, int totalBufferSize = 128000) 
        {
            var newSocketServer = new SocketCreator(recieveFunction, endPoint) 
            {
                bufferSize = totalBufferSize
            };
            newSocketServer.CurrentSocket = CreateSocket(protocol);
            newSocketServer.CurrentSocket.Bind(newSocketServer.EndPoint);

            Task.Run(() => newSocketServer.StartServerAsync());
            return newSocketServer;
        }

        protected static Socket CreateSocket(ProtocolType protocol) 
        {
            var socketType = protocol == ProtocolType.Udp ? SocketType.Dgram : SocketType.Stream;
            var socketOption = protocol == ProtocolType.Udp ? SocketOptionName.Broadcast : SocketOptionName.NoDelay;

            var newSocket = new Socket(AddressFamily.InterNetwork, socketType, protocol);
            newSocket.SetSocketOption(SocketOptionLevel.Socket, socketOption, true);

            return newSocket;
        }

        public async Task SendDatagramAsync(object datagram) 
        {
            var serializedData = DatagramHelper.WriteDatagram(datagram);
            await DatagramHelper.SendDatagramAsync(async data => await CurrentSocket.SendAsync(data, SocketFlags.None), serializedData);
        }

        public async Task SendToDatagramAsync(object datagram, EndPoint destination)
        {
            var serializedData = DatagramHelper.WriteDatagram(datagram);
            await DatagramHelper.SendDatagramAsync(async data => await CurrentSocket.SendToAsync(data, SocketFlags.None, destination), serializedData);
        }

        protected override Task<ClientDatagram> StartRecievingAsync()
        {
            return base.StartRecievingAsync();
        }
    }

    public partial class SocketServer 
    {
        private sealed class SocketCreator : SocketServer 
        {
            public override Socket CurrentSocket { get; set; }

            private Func<object, EndPoint, Task> _recieveFunction;

            public SocketCreator(Func<object, EndPoint, Task> recieveFunction, IPEndPoint endPoint) : base(endPoint.Address) 
            {
                _recieveFunction = recieveFunction;
                EndPoint = endPoint;
            }

            public SocketCreator(Func<object, EndPoint, Task> recieveFunction, IPAddress address, int port) : base(address) 
            {
                _recieveFunction = recieveFunction;
                EndPoint = new IPEndPoint(address, port);
            }

            public override async Task OnRecieveAsync(object datagram, EndPoint ipAddress)
            {
                await _recieveFunction(datagram, ipAddress);
            }
        }
    }
}
