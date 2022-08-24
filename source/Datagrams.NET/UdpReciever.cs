using DatagramsNet.Attributes;
using DatagramsNet.Datagram;
using DatagramsNet.Logging;
using DatagramsNet.Serialization;
using System.Net;
using System.Net.Sockets;

namespace DatagramsNet
{
    public sealed class UdpReciever
    {
        private readonly Socket _listeningSocket;

        public UdpReciever(Socket listeningSocket)
        {
            _listeningSocket = listeningSocket;
        }

        public async Task<bool> StartRecievingAsync(Func<object, EndPoint, Task> datagramAction, Func<Task<ClientDatagram>> clientData, bool consoleWriter = true)
        {
            if (consoleWriter)
            {
                await Task.Run(() => ServerLogger.StartConsoleWriter());
            }

            while (true)
            {
                var data = await clientData();
                Type dataType = DatagramHelper.GetBaseDatagramType(data.Datagram[0], typeof(PacketAttribute));

                var datagram = Serializer.DeserializeBytes(dataType, data.Datagram);
                if (datagram is not null)
                {
                    await datagramAction(datagram, data.Client);
                }
            }
        }

        public async Task<ClientDatagram> GetDatagramDataAsync()
        {
            Memory<byte> datagramMemory = new byte[4096];
            EndPoint currentEndPoint = (EndPoint)new IPEndPoint(IPAddress.Any, 0);
            var dataTask = await _listeningSocket.ReceiveFromAsync(datagramMemory, SocketFlags.None, currentEndPoint);

            SocketReceiveFromResult result = dataTask;
            return new ClientDatagram() { Client = (IPEndPoint)result.RemoteEndPoint, Datagram = datagramMemory.Span.ToArray() };
        }
    }
}
