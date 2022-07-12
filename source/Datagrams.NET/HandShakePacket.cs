using DatagramsNet.Attributes;
using DatagramsNet.Interfaces;
using System.Runtime.InteropServices;

namespace DatagramsNet
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
    public struct ConnectionKey<T> 
    {
        public T Key { get; set; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ShakeMessage 
    {
        public int IdMessage { get; set; }

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string? Message;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public ConnectionKey<TimeSpan>[] Keys = new ConnectionKey<TimeSpan>[] { new ConnectionKey<TimeSpan>() { Key = TimeSpan.Zero } };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class HandshakePacket : IDatagram
    {
        public int ProperId { get; }

        [Field(0)]
        public readonly int Id = 17;

        [Field(1), MarshalAs(UnmanagedType.LPStruct)]
        public readonly ShakeMessage Message;

        public HandshakePacket()
        {
            Message = new ShakeMessage();
        }

        public HandshakePacket(ShakeMessage message)
        {
            Message = message;
        }
    }
}
