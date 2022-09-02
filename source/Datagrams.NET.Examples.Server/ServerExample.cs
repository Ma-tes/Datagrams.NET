using System.Net;
using System.Net.Sockets;
using DatagramsNet.Logging;
using DatagramsNet.Prefixes;

namespace DatagramsNet.Examples.Server
{
    internal sealed class ServerExample : SocketServer
    {
        public override int PortNumber => base.PortNumber;
        public override Socket CurrentSocket { get; set; } =
            new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public int handShakeCounter = 0;

        protected override int bufferSize { get; set; } = 4096;

        public ServerExample(IPAddress ipAddress) : base(ipAddress) 
        {
            CurrentSocket.Bind(EndPoint);
            CurrentSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
            //CurrentSocket.Blocking = false;
            SocketReciever = new SocketReciever(CurrentSocket, bufferSize);
        }

        public override async Task OnRecieveAsync(object datagram, EndPoint ipAddress) 
        {
            if (datagram is HandshakePacket newDatagram)
            {
                handShakeCounter++;
                ServerLogger.Log<NormalPrefix>($"Id: {handShakeCounter} packet: {newDatagram.GetType()} testMessage: {newDatagram.ShortMessage} shakeMessage: [{newDatagram.Message.IdMessage}] -> {newDatagram.Message.Message} type: {newDatagram.Key}", TimeFormat.Half);
                for (int i = 0; i < newDatagram.Message.Keys.Length; i++)
                {
                    ServerLogger.Log<NormalPrefix>($"Key:[{i}] -> {newDatagram.Message.Keys[i]}", TimeFormat.Half);
                }
                await SendToDatagramAsync(datagram, ipAddress);
            }
        }
    }
}
