using DatagramsNet;
using System.Net;

const int DatagramCount = 100;
const int Port = 1111;

Console.Write("Target IP address (empty for local): ");
string? ipAddress = Console.ReadLine();
IPAddress ip = string.IsNullOrWhiteSpace(ipAddress) ? IPAddress.Loopback : IPAddress.Parse(ipAddress);

var client = Client.CreateUdp(ip, Port);
Parallel.For(0, DatagramCount, async i =>
{
    string message = $"ShortMessageTest[{i}]";
    string[] keys = new string[(i / 10) + 1];
    for (int j = 0; j < keys.Length; j++)
    {
        keys[j] = $"Key({j})";
    }
    var datagram = new HandshakePacket(new ShakeMessage() { IdMessage = i, Message = $"{message}", Keys = keys });
    await client.SendAsync(datagram);
});
Console.ReadKey(intercept: true);
