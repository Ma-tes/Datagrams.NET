using DatagramsNet.Attributes;
using DatagramsNet.Datagram;
using DatagramsNet.Logging;
using DatagramsNet.Serialization;
using System.Net;
using System.Net.Sockets;

namespace DatagramsNet
{
    public readonly struct SubDatagramTable
    {
        public byte[] Bytes { get; }
        public int Length { get; }

        public SubDatagramTable(byte[] bytes, int length)
        {
            Bytes = bytes;
            Length = length;
        }
    }

    public sealed class SocketReciever
    {
        private readonly Socket _listeningSocket;
        private bool isCanceled = false;

        public int BufferSize { get; }

        public SocketReciever(Socket listeningSocket, int bufferSize)
        {
            _listeningSocket = listeningSocket;
            BufferSize = bufferSize;
        }

        public async Task<bool> StartRecievingAsync(Func<object, EndPoint, Task> datagramAction, Func<Task<ClientDatagram>> clientData, Func<bool> cancelFunction, bool consoleWriter = true)
        {
            if (consoleWriter)
            {
                await Task.Run(() => ServerLogger.StartConsoleWriter());
            }

            while (true)
            {
                if (cancelFunction.Invoke()) 
                {
                    isCanceled = true;
                    _listeningSocket.Close();
                    return false;
                }

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
            Memory<byte> datagramMemory = new byte[BufferSize];
            EndPoint currentEndPoint = (EndPoint)new IPEndPoint(IPAddress.Any, 0);

            if (isCanceled)
                return default;

            SocketReceiveFromResult finalData = await _listeningSocket.ReceiveFromAsync(datagramMemory, SocketFlags.None, currentEndPoint);
            return new ClientDatagram() { Client = (IPEndPoint)finalData.RemoteEndPoint, Datagram = datagramMemory.Span.ToArray() };
        }
    }
}
