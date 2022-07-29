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

                int byteLength = (sizeof(int) + start);
                var newSpan = span[start..(sizeof(int) + byteLength)];
                size = MemoryMarshal.Read<int>(newSpan);
                bytes = memoryBytes.RemoveAtIndexes(sizeof(int), start);

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
            int offset = 0;

            foreach (var member in members)
            {
                var tableHolder = GetTableHolderInformation(member, bytesCopy, totalSize);
                int difference = bytesCopy.Length - tableHolder.Bytes.Length;

                bytesCopy = tableHolder.Bytes;
                totalSize += tableHolder.Length;
                offset += difference;
            }
            return totalSize + offset;
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

        public static byte[] RemoveAtIndexes(this Memory<byte> bytes, int length, int shift) 
        {
            Memory<byte> holder = bytes;
            for (int i = 0; i < length; i++)
            {
                TryRemoveAt(ref holder, shift);
            }
            return holder.ToArray();
        }

        private static bool TryRemoveAt(ref Memory<byte> bytes, int index)
        {
            Memory<byte> spanResult = new byte[bytes.Length - 1];

            if (index != 0) 
            {
                var spanSlice = bytes.Span[0..index];
                spanSlice.CopyTo(spanResult.Span);
            }

            int currentIndex = (index + 1);
            Memory<byte> newSpanSlice = bytes.Span[currentIndex..].ToArray();
            bool bytesCheck = ReplaceBytes(ref spanResult, newSpanSlice);

            if(bytesCheck)
                bytes = spanResult;
            return bytesCheck;
        }

        private static bool ReplaceBytes([NotNull] ref Memory<byte> source, Memory<byte> newData) 
        {
            if (source.Span.Length < newData.Span.Length)
                return false;

            Span<byte> span = source.Span;
            int lengthDifference = (source.Length - newData.Length);
            for (int i = 0; i < newData.Length; i++)
            {
                int index = i + (lengthDifference);
                span[index] = newData.Span[i];
            }
            source = span.ToArray();
            return true;
        }

        private static IntPtr GetIntPtr(int size) => Marshal.AllocHGlobal(size);
    }
}
