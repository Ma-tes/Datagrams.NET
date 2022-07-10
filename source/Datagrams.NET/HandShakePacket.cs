using DatagramsNet.Interfaces;
using System.Runtime.InteropServices;
using DatagramsNet.Attributes;

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
        public string Message;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public ConnectionKey<TimeSpan>[] Keys = new ConnectionKey<TimeSpan>[] { new ConnectionKey<TimeSpan>() { Key = TimeSpan.Zero } };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class HandShakePacket : IDatagram
    {
        public int ProperId { get; }

        [Field(0)]
        public int Id = 17;

        [MarshalAs(UnmanagedType.LPStruct)]
        [Field(1)]
        public ShakeMessage Message;

        public HandShakePacket() { }

        public HandShakePacket(ShakeMessage message) => Message = message;
    }
}
