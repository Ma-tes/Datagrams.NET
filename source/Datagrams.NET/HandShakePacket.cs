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

        public int Message { get; set; } = 128;

        //public ConnectionKey<TimeSpan>[] Keys = new ConnectionKey<TimeSpan>[] { new ConnectionKey<TimeSpan>() { Key = TimeSpan.Zero } };
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class HandShakePacket
    {
        [Field(0)]
        public int Id = 17;

        [Field(1)]
        public string ShortMessage = "Test";

        [Field(2)]
        public ShakeMessage Message = new();

        public HandShakePacket() { }

        public HandShakePacket(ShakeMessage message) => Message = message;
    }
}
