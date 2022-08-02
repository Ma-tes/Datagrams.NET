using DatagramsNet.Attributes;
using DatagramsNet.Interfaces;
using System.Runtime.InteropServices;

namespace DatagramsNet
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public sealed class KeyHolder
    {
        public int Key { get; set; }

        public string Value { get; set; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class ShakeMessage 
    {
        public int IdMessage { get; set; }
        public string Message { get; set; } = default!;
        public string[] Keys { get; set; } = new string[] { "One", "Two", "Three"};
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [Packet]
    public sealed class HandshakePacket : IDatagram
    {
        [Field(0)]
        public int Id { get; set; } = 17;

        [Field(1)]
        public string ShortMessage { get; set; } = "Test";

        [Field(2)]
        public ShakeMessage Message { get; set; } = new();

        [Field(3)]
        public int[] Values { get; set; } = new int[] { 1, 7 };

        [Field(4)]
        public KeyHolder[] Keys { get; set; } = new KeyHolder[] { new KeyHolder() { Key = 1, Value = "Test"}, new KeyHolder() { Key = 7, Value = "Test"} };

        public HandshakePacket() { }

        public HandshakePacket(ShakeMessage message) => Message = message;
    }
}
