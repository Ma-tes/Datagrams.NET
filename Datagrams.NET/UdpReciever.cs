using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using DatagramsNet.Attributes;

namespace DatagramsNet
{
    public sealed class UdpReciever : DatagramHolder
    {
        private Socket _listeningSocket;

        protected override TimeSpan datagramHoldTime => TimeSpan.FromMinutes(1);

        protected override int maxDatagramId => 255;

        public IPEndPoint EndPoint { get; init; }

        public UdpReciever(Socket listeningSocket) 
        {
            _listeningSocket = listeningSocket;
        }

        public async Task<bool> StartRecievingAsync(Func<object, Task> datagramAction)
        {
            List<byte[]> recievedDatagram = new();
            var indetificator = new DatagramIdentificator(recievedDatagram);
            await Task.Run(() => ServerLogger.StartConsoleWriter());
            while (true) 
            {
                var data = await GetDatagramDataAsync();
                Type dataType = DatagramHelper.GetBaseDatagramType(data.Datagram[0], typeof(PacketAttribute));
                var newData = indetificator.DeserializeDatagram(data.Datagram, GetSubBytesLength(dataType).ToArray());

                if (newData is not null)
                {
                    object datagram = DatagramHelper.ReadDatagram(newData.ToList());
                    if (datagram is not null) 
                    {
                        await datagramAction(datagram);
                    }
                    CurrentData = null;
                }
            }
            return true;
        }

        private IEnumerable<int> GetSubBytesLength(Type datagramType) 
        {
            var fields = datagramType.GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                yield return Marshal.SizeOf(fields[i].FieldType);
            }
        }

        private async Task<ClientDatagram> GetDatagramDataAsync() 
        {
            Memory<byte> datagramMemory = new byte[4096];
            EndPoint currentEndPoint = (EndPoint)new IPEndPoint(IPAddress.Any, 0);
            var dataTask = await _listeningSocket.ReceiveFromAsync(datagramMemory, SocketFlags.None, currentEndPoint);

            SocketReceiveFromResult result = dataTask;
            return new ClientDatagram() { Client = (IPEndPoint)result.RemoteEndPoint, Datagram = datagramMemory.Span.ToArray() };
        }
    }
}
