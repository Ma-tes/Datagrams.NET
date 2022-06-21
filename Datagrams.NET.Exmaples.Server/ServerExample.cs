using System.Net;
using DatagramsNet;
using DatagramsNet.Datagrams.NET.Logger;
using DatagramsNet.Datagrams.NET.Prefixes;

namespace Datagrams.NET.Examples.Server
{
    internal sealed class ServerExample : ServerManager
    {
        public override int PortNumber => base.PortNumber;

        public ServerExample(string name, IPAddress ipAddress) : base(name, ipAddress) { }

        public int handShakeCounter = 0;

        public override async Task OnRecieveAsync(object datagram, EndPoint ipAddress) 
        {
            if (datagram is HandShakePacket newDatagram)
            {
                handShakeCounter++;
                await ServerLogger.Log<NormalPrefix>($"Id: {handShakeCounter} packet: {newDatagram.GetType()}", TimeFormat.HALF);
            }
        }
    }
}
