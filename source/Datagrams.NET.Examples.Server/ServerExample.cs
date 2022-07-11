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
        public int HandshakeCounter { get; set; }

        private static ServerExample? serverHolder;

        public ServerExample(string name, IPAddress ipAddress) : base(name, ipAddress)
        {
            serverHolder = this;
        }

        public override async Task OnRecieveAsync(object datagram, EndPoint ipAddress)
        {
            if (datagram is HandshakePacket newDatagram)
            {
                HandshakeCounter++;
                await ServerLogger.LogAsync<NormalPrefix>($"Id: {HandshakeCounter} packet: {newDatagram.GetType()}", TimeFormat.Half);
            }
        }

        [CommandFunction<HelpCommand>]
        public static void WriteServerInformation()
        {
            Debug.Assert(serverHolder is not null);
            _ = ServerLogger.LogAsync<NormalPrefix>($"Connected: {serverHolder.ServerSocket.Connected}", TimeFormat.Half);
        }
    }
}
