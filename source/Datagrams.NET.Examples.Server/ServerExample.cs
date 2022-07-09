using DatagramsNet.Logging;
using DatagramsNet.Logging.Reading.CommandExecution;
using DatagramsNet.Logging.Reading.Commands;
using DatagramsNet.Prefixes;
using System.Diagnostics;
using System.Net;

namespace DatagramsNet.Examples.Server
{
    internal sealed class ServerExample : ServerManager
    {
        public override int PortNumber => base.PortNumber;
        public int HandShakeCounter { get; set; } = 0;

        private static ServerExample? serverHolder;

        public ServerExample(string name, IPAddress ipAddress) : base(name, ipAddress)
        {
            serverHolder = this;
        }

        public override async Task OnRecieveAsync(object datagram, EndPoint ipAddress)
        {
            if (datagram is HandshakePacket newDatagram)
            {
                HandShakeCounter++;
                await ServerLogger.LogAsync<NormalPrefix>($"Id: {HandShakeCounter} packet: {newDatagram.GetType()}", TimeFormat.Half);
            }
        }

        [CommandFunction<HelpCommand>()]
        public static void WriteServerInformation()
        {
            Debug.Assert(serverHolder is not null);

            var serverConnectionCount = serverHolder.ServerSocket.Available;
            Task.Run(async () => await ServerLogger.LogAsync<NormalPrefix>($"Connected: {serverHolder.ServerSocket.Connected}", TimeFormat.Half));
        }
    }
}
