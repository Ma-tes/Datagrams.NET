using DatagramsNet.Serialization.Attributes;
using DatagramsNet.Serialization.Interfaces;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DatagramsNet.Serialization.Types
{
    [TypeSerializer(typeof(Array))]
    internal sealed class GenericArrayType : ManagedTypeSerializer
    {
        private static readonly MethodInfo readMethodInfo = typeof(BinaryHelper).GetMethod(nameof(BinaryHelper.Read))!;
        private static readonly MethodInfo elementsMethodInfo = typeof(GenericArrayType).GetMethod(nameof(GenericArrayType.GetArrayElements))!;

        public override byte[] Serialize<TParent>(SizedObject @object)
        {
            var objectArray = (Array)@object.Value!;
            Type elementType = (@object.Value!).GetType().GetElementType()!;
            int byteLength = @object.Size;


            int memorySize = byteLength;
            memorySize += GetAdditionalSizeOf(elementType, objectArray.Length);
            var bytes = new byte[sizeof(int) + memorySize];

            Span<byte> spanBytes = bytes;
            MemoryMarshal.Write(spanBytes, ref memorySize);

            int totalSize = 0;
            for (int i = 0; i < objectArray.Length; i++)
            {
                object currentValue = objectArray.GetValue(i)!;
                Span<byte> span = BinaryHelper.Write(currentValue);

                span.CopyTo(spanBytes);
                totalSize += span.Length;
            }
            return bytes.ToArray();
        }

        public override T Deserialize<T>(byte[] bytes)
        {
            Type elementType = typeof(T).GetElementType()!;

            var subDatagram = ((IEnumerable<byte[]>)elementsMethodInfo.MakeGenericMethod(elementType).Invoke(null, new object[] { bytes })!).ToArray();//GetArrayElements(bytes, arrayHolder).ToArray();
            var elements = Array.CreateInstance(elementType, subDatagram.Length);
            for (int i = 0; i < subDatagram.Length; i++)
            {
                var @object = readMethodInfo.MakeGenericMethod(elementType).Invoke(null, new object[] { subDatagram[i] });
                elements.SetValue(@object!, i);
            }
            return (T)(object)(elements);
        }

        public static IEnumerable<ReadOnlyMemory<byte>> GetArrayElements<TElement>(ReadOnlyMemory<byte> bytes)
        {
            object? nullHolder = typeof(TElement).IsClass && !(Serializer.TryGetManagedType(typeof(TElement), out IManagedSerializer? _)) ?
                Activator.CreateInstance<TElement>() :
                null;

            if (typeof(TElement).IsClass)
            {
                while (!bytes.IsEmpty)
                {
                    int size = BinaryHelper.GetSizeOf(nullHolder, typeof(TElement), ref bytes);
                    yield return bytes[..size];
                    bytes = bytes[size..];
                }
            }
            else
            {
                int size = Unsafe.SizeOf<TElement>();
                while (!bytes.IsEmpty)
                {
                    yield return bytes[..size];
                    bytes = bytes[size..];
                }
            }
        }

        private static int GetAdditionalSizeOf(Type elementType, int length)
        {
            int size;
            if (Serializer.TryGetManagedType(elementType, out IManagedSerializer _))
                size = sizeof(int);
            else
               size = GetClassAdditionalSize(BinaryHelper.GetMembersInformation(Activator.CreateInstance(elementType)!));
            return length * size;
        }

        private static int GetClassAdditionalSize(ReadOnlySpan<MemberInformation> members) 
        {
            int totalSize = 0;
            for (int i = 0; i < members.Length; i++)
            {
                if (Serializer.TryGetManagedType(members[i].MemberType, out IManagedSerializer _))
                    totalSize += sizeof(int);
            }
            return totalSize;
        }
    }
}
