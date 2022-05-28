using System.Net;
using System.Net.Sockets;

namespace DatagramsNet
{
    public class Client : UdpClient
    {
        public string Name { get; set; }

        public IPAddress IpAddress { get; set; }

        public int PortNumber { get; set; }

        public IPEndPoint EndPoint => new IPEndPoint(IpAddress, PortNumber);

        public Client(string name, IPAddress ipAddress, int portNumber) 
        {
            Name = name;
            PortNumber = portNumber;
            IpAddress = ipAddress;
            if (Available == 0)
            {
                Connect(IpAddress, portNumber);
                Task.Run(() => CheckConnection());
            }
        }

        private async Task CheckConnection(string message = "Client is connected") 
        {
            var handShakePacket = new HandShakePacket(new ShakeMessage() {IdMessage = 17, Message = message.ToCharArray() });
            var writer = DatagramHelper.WriteDatagram(handShakePacket);
            await DatagramHelper.SendDatagramAsync(new Func<byte[], Task>(async(byte[] bytes) => await SendAsync(bytes)), writer);
        }
    }
}
