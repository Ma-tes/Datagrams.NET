using DatagramsNet;
using DatagramsNet.Datagram;
using System.Net;

const int DatagramCount = 100;
const int Port = 1111;

Console.Write("Target IP address (empty for local): ");
string? ipAddress = Console.ReadLine();
IPAddress ip = string.IsNullOrWhiteSpace(ipAddress) ? IPAddress.Loopback : IPAddress.Parse(ipAddress);

var client = new Client("TestClient", ip, Port);
Parallel.For(0, DatagramCount, async i =>
{
    var datagram = new HandshakePacket(new ShakeMessage() { IdMessage = i, Message = "Client Message12345" });
    await DatagramHelper.SendDatagramAsync(async data => await client.SendAsync(data), data: DatagramHelper.WriteDatagram(datagram));
});
