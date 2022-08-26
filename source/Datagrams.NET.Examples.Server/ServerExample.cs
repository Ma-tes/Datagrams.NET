using System.Net;
using DatagramsNet.Logging;
using DatagramsNet.Prefixes;

namespace DatagramsNet.Examples.Server
{
    internal sealed class ServerExample : ServerManager
    {
        public override int PortNumber => base.PortNumber;
        public int handShakeCounter = 0;

        public ServerExample(string name, IPAddress ipAddress) : base(name, ipAddress) 
        {
        }

        public override Task OnRecieveAsync(object datagram, EndPoint ipAddress) 
        {
            if (datagram is HandshakePacket newDatagram)
            {
                handShakeCounter++;
                ServerLogger.Log<NormalPrefix>($"Id: {handShakeCounter} packet: {newDatagram.GetType()} testMessage: {newDatagram.ShortMessage} shakeMessage: [{newDatagram.Message.IdMessage}] -> {newDatagram.Message.Message} type: {newDatagram.Key}", TimeFormat.Half);
                for (int i = 0; i < newDatagram.Message.Keys.Length; i++)
                {
                    ServerLogger.Log<NormalPrefix>($"Key:[{i}] -> {newDatagram.Message.Keys[i]}", TimeFormat.Half);
                }
            }
            return Task.CompletedTask;
        }
    }
}
