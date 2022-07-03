﻿using System.Net;
using DatagramsNet;
using DatagramsNet.Datagrams.NET.Logger;
using DatagramsNet.Datagrams.NET.Logger.Reader.CommandExecuting;
using DatagramsNet.Datagrams.NET.Logger.Reader.Commands;
using DatagramsNet.Datagrams.NET.Prefixes;

namespace Datagrams.NET.Examples.Server
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
                await ServerLogger.Log<NormalPrefix>($"Id: {handShakeCounter} packet: {newDatagram.GetType()}", TimeFormat.HALF);
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
