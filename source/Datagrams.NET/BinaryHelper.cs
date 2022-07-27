using DatagramsNet.Serialization;
using DatagramsNet.Serialization.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DatagramsNet
{
    internal readonly struct ObjectTableSize
    {
        public object? Value { get; }
        public int Size { get; }

        public ObjectTableSize(object? value, int size)
        {
            Value = value;
            Size = size;
        }
    }

    public readonly struct MemberTableHolder
    {
        public byte[] Bytes { get; }
        public int Length { get; }

        public MemberTableHolder(byte[] bytes, int length)
        {
            Bytes = bytes;
            Length = length;
        }
    }

    public static class BinaryHelper
    {
        private static readonly MethodInfo deserialization = typeof(ManagedTypeFactory).GetMethod(nameof(ManagedTypeFactory.Deserialize))!;

        private static readonly int[] defaultIntIndexes = { 0, 1, 2, 3 };

        public static byte[] Write(object @object)
        {
            byte[] byteHolder = Array.Empty<byte>();
            int size = GetSizeOf(@object, ref byteHolder);
            var buffer = Serializer.SerializeObject(@object, size);

            if (buffer is null)
            {
                var fixedBuffer = new byte[size];

                IntPtr pointer = GetIntPtr(size);
                Marshal.StructureToPtr(@object, pointer, false);
                Marshal.Copy(pointer, fixedBuffer, 0, fixedBuffer.Length);
                return fixedBuffer;
            }

            return buffer;
        }

        public static T Read<T>(byte[] bytes)
        {
            IManaged? managedType;
            Serializer.TryGetManagedType(typeof(T), out managedType);
            if (managedType is not null)
            {
                var currentObject = (T)(deserialization.MakeGenericMethod(typeof(T)).Invoke(null, new object[] { managedType, bytes }))!;
                return currentObject;
            }

            var @object = Activator.CreateInstance<T>();
            var members = GetMembersInformation(@object!).ToArray();
            if (members is not null)
            {
                var @currentObject = (T)Serializer.DeserializeBytes(@object!.GetType(), bytes);
                return @currentObject;
            }

            IntPtr objectPointer = GetIntPtr(bytes.Length);
            Marshal.Copy(bytes, 0, objectPointer, bytes.Length);
            return (T)Marshal.PtrToStructure(objectPointer, typeof(T))!;
        }

        public static ReadOnlySpan<MemberInformation> GetMembersInformation(object @object)
        {
            var members = GetMembers(@object.GetType());
            if (members is null)
                return null;

            int membersCount = members.Count;
            var membersInformations = new MemberInformation[membersCount];
            for (int i = 0; i < membersCount; i++)
            {
                var currentMember = members[i];
                Type memberType = currentMember.MemberType is MemberTypes.Property ? ((PropertyInfo)currentMember).PropertyType : ((FieldInfo)currentMember).FieldType;
                object? memberObject = currentMember.MemberType is MemberTypes.Property ? ((PropertyInfo)currentMember).GetValue(@object) : ((FieldInfo)currentMember).GetValue(@object);

                MemberInformation memberInformation = new(memberObject!, memberType);
                membersInformations[i] = memberInformation;
            }
            return (ReadOnlySpan<MemberInformation>)membersInformations;
        }

        private static List<MemberInfo> GetMembers(Type objectType)
        {
            if (!(objectType.IsClass))
                return null!;
            var members = new List<MemberInfo>();
            members.AddRange(objectType.GetFields());
            members.AddRange(objectType.GetProperties());
            return members;
        }

        public static int GetSizeOf<T>(T @object, ref byte[] bytes)
        {
            int size;
            if (@object is not null && @object.GetType().IsClass && @object is not string && @object.GetType().BaseType != typeof(Array))
            {
                var membersInformation = GetMembersInformation(@object!).ToArray();
                var sizeTable = GetSizeOfClass(membersInformation, bytes);
                size = sizeTable;
            }
            else
            {
                var holder = GetTableHolderInformation(new MemberInformation(@object, @object!.GetType()), bytes, 0);
                size = holder.Length;
                bytes = holder.Bytes;
            }
            return size;
        }


        private static MemberTableHolder GetTableHolderInformation(MemberInformation member, byte[] bytes, int start)
        {
            object memberObject = member.MemberValue!;
            Memory<byte> memoryBytes = bytes;
            int size;

            if ((memberObject is NullValue || Serializer.TryGetManagedType(member.MemberType, out IManaged? _)) && bytes.Length > 1)
            {
                ReadOnlySpan<byte> span = bytes;

                size = MemoryMarshal.Read<int>(span[0..sizeof(int)]);
                bytes = memoryBytes.RemoveAtIndexes(defaultIntIndexes, start);

                return new MemberTableHolder(bytes, size);
            }

            if (memberObject is string text)
            {
                size = text.Length * sizeof(byte);
                return new MemberTableHolder(bytes, size);
            }

            if (memberObject is Array || member.MemberType.BaseType == typeof(Array))
            {
                var memberArray = ((Array)memberObject!);
                size = GetSizeOfArray(memberArray, ref bytes);
                return new MemberTableHolder(bytes, size);
            }

            size = Marshal.SizeOf(memberObject!);
            return new MemberTableHolder(bytes, size);
        }

        private static int GetSizeOfClass(MemberInformation[] members, byte[] bytes)
        {
            byte[] bytesCopy = bytes;
            int totalSize = 0;
            int originalSize = 0;

            foreach (var member in members)
            {
                var currentBytesCopy = bytes;
                var tableHolder = GetTableHolderInformation(member, bytes, totalSize);

                totalSize += tableHolder.Length;
                originalSize += (tableHolder.Length + (bytesCopy.Length - tableHolder.Bytes.Length));
            }
            return originalSize;
        }

        public static int GetSizeOfArray(Array array, ref byte[] bytes)
        {
            int totalSize = 0;
            for (int i = 0; i < array.Length; i++)
            {
                int currentSize = GetSizeOf(array.GetValue(i), ref bytes);
                totalSize += currentSize;
            }
            return totalSize;
        }

        public static byte[] RemoveAtIndexes(this Memory<byte> bytes, int[] indexes, int shift) 
        {
            Memory<byte> holder = bytes;
            for (int i = 0; i < indexes.Length; i++)
            {
                int currentIndex = shift + indexes[i];
                if (TryRemoveAt(ref holder, currentIndex))
                    bytes = holder;
            }
            return bytes.ToArray();
        }

        private static bool TryRemoveAt(ref Memory<byte> bytes, int index)
        {
            if (index < 0 && index > bytes.Length)
                return false;
            var result = new byte[bytes.Length - 1];
            Span<byte> spanResult = result.AsSpan();

            if (index != 0)
            {
                Span<byte> firstSpan = bytes.Span.Slice(0, (index));
                firstSpan.CopyTo(spanResult);
            }
            Span<byte> secondSpan = bytes.Span.Slice((index + 1), (bytes.Length - (index + 1)));
            secondSpan.CopyTo(spanResult);
            bytes = spanResult.ToArray();

            return spanResult.Length < bytes.Length;
        }

        private static IntPtr GetIntPtr(int size) => Marshal.AllocHGlobal(size);
    }
}
