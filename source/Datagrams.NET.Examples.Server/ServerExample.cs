using System.Net;
using DatagramsNet.Logging;
using DatagramsNet.Prefixes;

namespace DatagramsNet.Examples.Server
{
    internal sealed class ServerExample : ServerManager
    {
        private static ServerExample serverHolder;

        public override int PortNumber => base.PortNumber;
        public int handShakeCounter = 0;

        public ServerExample(string name, IPAddress ipAddress) : base(name, ipAddress) { serverHolder = this; }

        public override async Task OnRecieveAsync(object datagram, EndPoint ipAddress) 
        {
            if (datagram is HandShakePacket newDatagram)
            {
                handShakeCounter++;
                await ServerLogger.LogAsync<NormalPrefix>($"Id: {handShakeCounter} packet: {newDatagram.GetType()} testMessage: {newDatagram.ShortMessage}", TimeFormat.Half);
                for (int i = 0; i < newDatagram.Message.Keys.Length; i++)
                {
                    await ServerLogger.LogAsync<NormalPrefix>($"Key:[{i}] -> {newDatagram.Message.Keys[i]}", TimeFormat.Half);
                }
                //await ServerLogger.Log<NormalPrefix>($"Id: {handShakeCounter} packet: {newDatagram.GetType()} message: {newDatagram.ShortMessage}", TimeFormat.HALF);
            }
        }
    }
}
