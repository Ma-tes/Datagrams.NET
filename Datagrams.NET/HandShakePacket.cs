using DatagramsNet.Interfaces;
using System.Runtime.InteropServices;
using DatagramsNet.Attributes;

namespace DatagramsNet
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ShakeMessage 
    {
        public int IdMessage { get; set; }

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)]
        public char[] Message;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class HandShakePacket : IDatagram
    {
        public int ProperId { get; }

        [Field(0)]
        public int Id = 17;

        [Field(1)]
        public ShakeMessage Message;

        public HandShakePacket() { }

        public HandShakePacket(ShakeMessage message) => Message = message;
    }
}
