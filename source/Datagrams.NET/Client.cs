using System.Net;
using System.Net.Sockets;

namespace DatagramsNet
{
    public class Client : UdpClient
    {
        public string Name { get; }

        private IPEndPoint endPoint;

        public Client(string name, IPAddress ipAddress, int portNumber)
        {
            Name = name;
            endPoint = new IPEndPoint(ipAddress, portNumber);
            if (Available == 0)
            {
                Connect(endPoint.Address, portNumber);
                Task.Run(() => CheckConnection());
            }
        }

        protected virtual async Task CheckConnection(string message = "Client is connected")
        {
            var handShakePacket = new HandshakePacket(new ShakeMessage() { IdMessage = 17, Message = message });
            //var writer = DatagramHelper.WriteDatagram(handShakePacket);
            //await DatagramHelper.SendDatagramAsync(new Func<byte[], Task>(async (byte[] bytes) => await SendAsync(bytes)), writer);
        }
    }
}
