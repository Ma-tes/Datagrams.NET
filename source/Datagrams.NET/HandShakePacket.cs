using DatagramsNet.Attributes;
using DatagramsNet.Interfaces;
using System.Runtime.InteropServices;

namespace DatagramsNet
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ShakeMessage
    {
        public int IdMessage { get; set; }

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string Message;
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
        }

        public HandshakePacket(ShakeMessage message)
        {
            Message = message;
        }
    }
}
