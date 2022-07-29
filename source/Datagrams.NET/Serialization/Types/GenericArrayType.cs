using DatagramsNet.Serialization.Attributes;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DatagramsNet.Serialization.Types
{
    [TypeSerializer(typeof(Array))]
    internal sealed class GenericArrayType : ManagedType
    {
        private static readonly MethodInfo read = typeof(BinaryHelper).GetMethod(nameof(BinaryHelper.Read))!;

        private readonly int intSize = sizeof(int);

        public override byte[] Serialize<TParent>(ObjectTableSize @object)
        {
            var objectArray = (Array)@object.Value!;
            int byteLength = @object.Size;


            int memorySize = (byteLength) + (objectArray.Length * sizeof(int));
            var bytes = new byte[memorySize + intSize];

            Span<byte> spanBytes = bytes;
            MemoryMarshal.Write(spanBytes, ref memorySize);

            int totalSize = 0;
            for (int i = 0; i < objectArray.Length; i++)
            {
                object currentValue = objectArray.GetValue(i)!;
                Span<byte> span = BinaryHelper.Write(currentValue).AsSpan<byte>();

                span.CopyTo(spanBytes[(intSize + totalSize)..]);
                totalSize += span.Length;
            }
            return bytes.ToArray();
        }

        public override T Deserialize<T>(byte[] bytes)
        {
            Type elementType = typeof(T).GetElementType()!;
            var subDatagram = GetArrayElements(bytes, elementType).ToArray();

            var elements = Array.CreateInstance(elementType, subDatagram.Length);
            for (int i = 0; i < subDatagram.Length; i++)
            {
                var @object = read.MakeGenericMethod(elementType).Invoke(null, new object[] { subDatagram[i] });
                elements.SetValue(@object!, i);
            }
            return (T)(object)(elements);
        }

        private static IEnumerable<byte[]> GetArrayElements(byte[] bytes, Type elementType)
        {
            int offset = 0;
            while (bytes.Length > 1)
            {
                var bytesCopy = bytes.Length;
                int size = BinaryHelper.GetSizeOf(new NullValue(), ref bytes);

                int difference = bytes.Length - size;
                size = difference < 0 ? size + difference : size;

                byte[] oldBytes = bytes[0..size];
                offset += bytesCopy - difference;
                bytes = bytes[size..];
                yield return oldBytes;
            }
        }
    }
}
