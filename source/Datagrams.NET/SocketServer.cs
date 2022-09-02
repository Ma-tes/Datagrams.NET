﻿using DatagramsNet.Datagram;
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

        public static SocketServer CreateServer(ProtocolType protocol, Func<object, EndPoint, Task> recieveFunction, IPAddress address, int totalBufferSize = 4096) 
        {
            var newSocketServer = new SocketCreator(recieveFunction, address) 
            {
                bufferSize = totalBufferSize
            };
            var socketType = protocol == ProtocolType.Udp ? SocketType.Dgram : SocketType.Stream;

            newSocketServer.CurrentSocket = new Socket(AddressFamily.InterNetwork, socketType, protocol);
            newSocketServer.CurrentSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            newSocketServer.CurrentSocket.Bind(newSocketServer.EndPoint);

            return newSocketServer;
        }

        public async Task SendDatagramAsync(object datagram) =>
            await SendToDatagramAsync(datagram, new IPEndPoint(IPAddress.Any, 0));

        public async Task SendToDatagramAsync(object datagram, EndPoint destination)
        {
            var serializedData = DatagramHelper.WriteDatagram(datagram);
            await DatagramHelper.SendDatagramAsync(async data => await this.CurrentSocket.SendToAsync(data, SocketFlags.None, destination), serializedData);
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

            public SocketCreator(Func<object, EndPoint, Task> recieveFunction, IPAddress address) : base(address) 
            {
                _recieveFunction = recieveFunction;
            }

            public override async Task OnRecieveAsync(object datagram, EndPoint ipAddress)
            {
                await _recieveFunction(datagram, ipAddress);
            }
        }
    }
}
