using DatagramsNet.Serializer.Attributes;
using System.Reflection;

namespace DatagramsNet.Serializer.Types
{
    [SerializeType(typeof(Array))]
    internal sealed class GenericArrayType : ManagedType
    {
        private static MethodInfo read = typeof(BinaryHelper).GetMethod(nameof(BinaryHelper.Read));

        public override byte[] Serialize<TParent>(ObjectTableSize @object)
        {
            //var baseSerialize = base.Serialize<TParent>(@object);
            //if (baseSerialize is not null)
            //return baseSerialize;

            var bytes = new List<byte>();
            bytes.Add((byte)@object.Size);
            var objectArray = (Array)@object.Value!;

            int totalSize = 0;
            for (int i = 0; i < objectArray.Length; i++)
            {
                object currentValue = objectArray.GetValue(i)!;
                byte[] currentBytes = BinaryHelper.Write(currentValue);

                totalSize += currentBytes.Length;
                bytes.AddRange(currentBytes);
            }
            int difference = totalSize - bytes[0];
            bytes[0] = (byte)(bytes[0] + (byte)difference);
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

        private IEnumerable<byte[]> GetArrayElements(byte[] bytes, Type elementType)
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
