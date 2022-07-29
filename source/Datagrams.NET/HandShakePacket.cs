using DatagramsNet.Attributes;
using DatagramsNet.Interfaces;
using System.Runtime.InteropServices;

namespace DatagramsNet
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ShakeMessage 
    {
        public int IdMessage { get; set; }

        public string Message { get; set; } = default;

        //public string[] Keys { get; set; } = new string[] { "Test", "Serialization", "AnotherTest"};
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class HandshakePacket : IDatagram
    {
        [Field(0)]
        public int Id => 17;

        [Field(1)]
        public string ShortMessage { get; set; } = "Test";

        [Field(2)]
        public ShakeMessage Message { get; set; } = new();

        //[Field(3)]
        //public int[] Values { get; set; } = new int[] { 1, 7 };

        public HandshakePacket() { }

        public HandshakePacket(ShakeMessage message) => Message = message;
    }
}
