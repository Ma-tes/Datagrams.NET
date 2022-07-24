using System.Runtime.InteropServices;
using DatagramsNet.Attributes;
using DatagramsNet.Interfaces;

namespace DatagramsNet
{
    public struct ConnectionKey<T> 
    {
        public T Key { get; set; }

        public string Message { get; set; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ShakeMessage 
    {
        public int IdMessage { get; set; }

        public string Message { get; set; } = default;

        //public ConnectionKey<TimeSpan>[] Keys = new ConnectionKey<TimeSpan>[] { new ConnectionKey<TimeSpan>() { Key = TimeSpan.Zero, Message = "TestMessage"} };
        public string[] Keys { get; set; } = new string[] { "Test", "Serialization", "AnotherTest"};
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class HandShakePacket : IDatagram
    {
        [Field(0)]
        public int Id => 17;

        [Field(1)]
        public string ShortMessage { get; set; } = "Test";

        [Field(2)]
        public ShakeMessage Message { get; set; } = new();

        public HandShakePacket() { }

        public HandShakePacket(ShakeMessage message) => Message = message;
    }
}
