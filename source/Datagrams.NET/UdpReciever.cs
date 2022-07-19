using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using DatagramsNet.Attributes;
using DatagramsNet.Datagram;
using DatagramsNet.Logging;

namespace DatagramsNet
{

    public sealed class UdpReciever
    {
        private Socket _listeningSocket;

        public UdpReciever(Socket listeningSocket) 
        {
            _listeningSocket = listeningSocket;
        }

        public async Task<bool> StartRecievingAsync(Func<object, EndPoint, Task> datagramAction, Func<Task<ClientDatagram>> clientData, bool consoleWriter = true)
        {
            List<byte[]> recievedDatagram = new();
            if (consoleWriter) 
            {
                await Task.Run(() => ServerLogger.StartConsoleWriter());
            }

            while (true) 
            {
                var data = await clientData();
                Type dataType = DatagramHelper.GetBaseDatagramType(data.Datagram[0], typeof(PacketAttribute));
                var newData = DatagramIdentificator.DeserializeDatagram(data.Datagram, GetSubBytesLength(dataType).ToArray());

                if (newData is not null)
                {
                    object datagram = DatagramHelper.ReadDatagram(newData.ToArray().AsMemory());
                    if (datagram is not null) 
                    {
                        await datagramAction(datagram, data.Client);
                    }
                }
            }
            return true;
        }

        private int[] GetSubBytesLength(Type datagramType) 
        {
            var datagramInstance = Activator.CreateInstance(datagramType);
            var membersInformation = BinaryHelper.GetMembersInformation(datagramInstance).ToArray();
            var subBytes = new int[membersInformation.Length];
            for (int i = 0; i < membersInformation.Length; i++)
            {
                int size = BinaryHelper.GetSizeOf(membersInformation[i].MemberValue);
                subBytes[i] = size;
            }
            return subBytes;
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
