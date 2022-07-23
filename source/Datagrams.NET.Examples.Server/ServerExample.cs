using System.Net;
using DatagramsNet.Logging;
using DatagramsNet.Logging.Reading.CommandExecuting;
using DatagramsNet.Logging.Reading.Commands;
using DatagramsNet.Prefixes;

namespace DatagramsNet.Examples.Server
{
    internal sealed class ServerExample : ServerManager
    {
        public override int PortNumber => base.PortNumber;

        private static ServerExample serverHolder;

        public int handShakeCounter = 0;

        public ServerExample(string name, IPAddress ipAddress) : base(name, ipAddress) { serverHolder = this; }

        public override async Task OnRecieveAsync(object datagram, EndPoint ipAddress) 
        {
            if (datagram is HandShakePacket newDatagram)
            {
                handShakeCounter++;
                await ServerLogger.Log<NormalPrefix>($"Id: {handShakeCounter} packet: {newDatagram.GetType()} testMessage: {newDatagram.ShortMessage} message: {newDatagram.Message.Message} array(length): {newDatagram.Message}", TimeFormat.HALF);
                //await ServerLogger.Log<NormalPrefix>($"Id: {handShakeCounter} packet: {newDatagram.GetType()} message: {newDatagram.ShortMessage}", TimeFormat.HALF);
            }
        }

        [CommandFunction<HelpCommand>()]
        public static void WriteServerInformation() 
        {
            var serverConnectionCount = serverHolder.serverSocket.Available;
            Task.Run(async() => await ServerLogger.Log<NormalPrefix>($"Connected: {serverHolder.serverSocket.Connected}", TimeFormat.HALF));
        }
    }
}
