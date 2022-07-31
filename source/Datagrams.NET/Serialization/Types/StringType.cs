using DatagramsNet.Serialization.Attributes;
using System.Runtime.InteropServices;
using System.Text;

namespace DatagramsNet.Serialization.Types
{
    [TypeSerializer(typeof(string))]
    internal sealed class StringType : ManagedTypeSerializer
    {
        private static readonly byte[] _emptyStringBytes = new byte[4];

        public override byte[] Serialize<TParent>(SizedObject @object)
        {
            if (@object.Value is not string text)
                return _emptyStringBytes;

            int byteCount = Encoding.UTF8.GetByteCount(text);
            byte[] bytes = new byte[sizeof(int) + byteCount];
            Span<byte> span = bytes;

            MemoryMarshal.Write(span, ref byteCount);
            Encoding.UTF8.GetBytes(text, span[sizeof(int)..]);

            return bytes;
        }

        public override T Deserialize<T>(byte[] bytes)
        {
            return (T)(object)Encoding.UTF8.GetString(bytes);
        }
    }
}
