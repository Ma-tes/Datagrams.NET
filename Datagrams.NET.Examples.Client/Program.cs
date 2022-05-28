using System.Net;
using DatagramsNet;

namespace Datagrams.NET.Examples.Clients 
{
    internal class Program 
    {
        private static Client client;

        public static void Main() 
        {
            string ipAddress = Console.ReadLine();
            client = new Client("TestClient", IPAddress.Parse(ipAddress), 1111);
            for (int i = 0; i < 100000; i++)
            {
                var datagram = new HandShakePacket(new ShakeMessage() {IdMessage = i, Message = "Client Message12345".ToCharArray() });
                    DatagramHelper.SendDatagramAsync(new Func<byte[], Task>(async (byte[] data) => await client.SendAsync(data)), DatagramHelper.WriteDatagram(datagram));
            }
            Console.ReadLine();
        }
    }
}
