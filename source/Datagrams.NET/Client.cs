using DatagramsNet.Datagram;
using System.Net;
using System.Net.Sockets;

namespace DatagramsNet
{
    public class Client : UdpClient
    {
        public string Name { get; }
        public IPAddress IpAddress { get; }
        public int PortNumber { get; }
        public IPEndPoint EndPoint { get; }

        public Client(string name, IPAddress ipAddress, int portNumber)
        {
            Name = name;
            PortNumber = portNumber;
            IpAddress = ipAddress;
            EndPoint = new IPEndPoint(IpAddress, PortNumber);
            if (Available == 0)
            {
                Connect(IpAddress, portNumber);
                _ = CheckConnectionAsync();
            }
        }

        protected virtual async Task CheckConnectionAsync(string message = "Client is connected")
        {
            var handShakePacket = new HandshakePacket(new ShakeMessage() { IdMessage = 17, Message = message });
            var writer = DatagramHelper.WriteDatagram(handShakePacket);
            await DatagramHelper.SendDatagramAsync(new Func<byte[], Task>(async (byte[] bytes) => await SendAsync(bytes)), writer);
        }
    }
}
