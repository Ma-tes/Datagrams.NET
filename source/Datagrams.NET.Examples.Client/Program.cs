using DatagramsNet;
using DatagramsNet.Logging;
using DatagramsNet.Prefixes;
using System.Net;

const int DatagramCount = 100;
const int Port = 1111;

Console.Write("Target IP address (empty for local): ");
string? ipAddress = Console.ReadLine();
IPAddress ip = string.IsNullOrWhiteSpace(ipAddress) ? IPAddress.Loopback : IPAddress.Parse(ipAddress);
var destination = new IPEndPoint(ip, Port);

var client = SocketServer.CreateServer(System.Net.Sockets.ProtocolType.Tcp, async(object datagram, EndPoint endPoint) =>
{
    if (datagram is HandshakePacket newDatagram) 
    {
        await ServerLogger.LogAsync<NormalPrefix>($"You recieved message from {endPoint.AddressFamily}", TimeFormat.Half);
    }
}, IPAddress.Any);
Task.Run(() => client.CurrentSocket.ConnectAsync(destination));

Parallel.For(0, DatagramCount, async i =>
{
    string message = $"ShortMessageTest[{i}]";
    string[] keys = new string[(i / 10) + 1];
    for (int j = 0; j < keys.Length; j++)
    {
        keys[j] = $"Key({j})";
    }

    var bytes = new byte[] { 255, 16, 128, 32, 64 };
    var datagram = new HandshakePacket(new ShakeMessage() { IdMessage = i, Message = $"{message}", Keys = keys, Bytes = bytes})
    {
        Key = typeof(HandshakePacket)
    };
    await client.SendToDatagramAsync(datagram, destination);
});
Console.ReadKey(intercept: true);
