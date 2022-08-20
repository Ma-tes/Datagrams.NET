using DatagramsNet.Attributes;
using DatagramsNet.Datagram;
using DatagramsNet.Interfaces;
using DatagramsNet.Serialization;
using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace DatagramsNet
{
    public sealed class Client : IDisposable
    {
        private readonly Socket socket;
        private readonly IPEndPoint remoteEndPoint;

        private Client(Socket socket, IPEndPoint remoteEndPoint)
        {
            this.socket = socket;
            this.remoteEndPoint = remoteEndPoint;
        }

        public static async Task<Client> ConnectTcpAsync(IPAddress address, int port)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(address, port);
            return new Client(socket, new IPEndPoint(address, port));
        }

        public static Client CreateUdp(IPAddress address, int port)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            return new Client(socket, new IPEndPoint(address, port));
        }

        public async ValueTask SendAsync<T>(T datagram, CancellationToken cancellationToken = default)
        {
            ReadOnlyMemory<byte> data = new DatagramIdentificator(DatagramHelper.WriteDatagram(datagram)).SerializeDatagram();
            await SendAsync(data, cancellationToken);
        }

        public async ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            while (!data.IsEmpty)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int bytesSent = (socket.ProtocolType == ProtocolType.Tcp) ?
                    await socket.SendAsync(data, SocketFlags.None, cancellationToken) :
                    await socket.SendToAsync(data, SocketFlags.None, remoteEndPoint, cancellationToken);
                data = data[bytesSent..];
            }
        }

        public async ValueTask<IDatagram> ReceiveAsync(CancellationToken cancellationToken = default)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(4096);
            Memory<byte> data = await ReceiveAsync(buffer, cancellationToken);
            Type dataType = DatagramHelper.GetBaseDatagramType(MemoryMarshal.Read<int>(data.Span), typeof(PacketAttribute));

            var deserializedObject = Serializer.DeserializeBytes(dataType, data);
            ArrayPool<byte>.Shared.Return(buffer);

            return deserializedObject as IDatagram ?? throw new Exception(); // TODO: Select better exception type
        }

        public async ValueTask<Memory<byte>> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int received = (socket.ProtocolType == ProtocolType.Tcp) ?
                await socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken) :
                (await socket.ReceiveFromAsync(buffer, SocketFlags.None, remoteEndPoint, cancellationToken)).ReceivedBytes;

            return buffer[..received];
        }

        public void Dispose()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Dispose();
        }
    }
}
