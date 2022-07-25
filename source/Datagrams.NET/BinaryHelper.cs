using DatagramsNet.Serializer;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DatagramsNet
{
    internal struct ObjectTableSize
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
        private static MethodInfo deserialization = typeof(ManagedTypeFactory).GetMethod(nameof(ManagedTypeFactory.Deserialize))!;

        public static byte[] Write(object @object)
        {
            byte[] byteHolder = new byte[0];
            int size = GetSizeOf(@object, ref byteHolder);
            var fixedBuffer = new byte[size];
            var buffer = Serialization.SerializeObject(@object, size);

            if (buffer is null) 
            {
                IntPtr pointer = GetIntPtr(size);
                Marshal.StructureToPtr(@object, pointer, false);
                Marshal.Copy(pointer, fixedBuffer, 0, fixedBuffer.Length);
                return fixedBuffer;
            }

            return buffer;
        }

        public static T Read<T>(byte[] bytes)
        {
            var managedType = Serialization.TryGetManagedType(typeof(T));
            if (managedType is not null) 
            {
                var currentObject = (T)(deserialization.MakeGenericMethod(typeof(T)).Invoke(null, new object[] { managedType, bytes}))!;
                return currentObject;
            }

            var @object = Activator.CreateInstance<T>();
            var members = GetMembersInformation(@object!).ToArray();
            if (members is not null) 
            {
                var @currentObject = (T)Serialization.DeserializeBytes(@object!.GetType(), bytes);
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
            var managedType = Serialization.TryGetManagedType(member.MemberType);

            int size;
            if ((memberObject is NullValue || managedType is not null) && bytes.Length > 1)
            {
                size = bytes[start];
                bytes = RemoveSizeIndex(bytes, start);

                return new MemberTableHolder(bytes, size);
            }

            if (memberObject is string)
            {
                size = (((string)(memberObject)).Length * sizeof(byte));
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

        private static int GetSizeOfArray(Array array, ref byte[] bytes) 
        {
            int totalSize = 0;
            for (int i = 0; i < array.Length; i++)
            {
                int currentSize = BinaryHelper.GetSizeOf(array.GetValue(i), ref bytes);
                totalSize += currentSize;
            }
            return totalSize;
        }

        private static byte[] RemoveSizeIndex(byte[] bytes, int index) 
        {
            var result = new byte[bytes.Length - 1];
            Span<byte> spanResult = result.AsSpan();
            Span<byte> byteSpan = bytes;

            if (index != 0) 
            {
                Span<byte> firstSpan = byteSpan.Slice(0, (index));
                firstSpan.CopyTo(spanResult);
            }
            Span<byte> secondSpan = byteSpan.Slice((index + 1), (bytes.Length - (index + 1)));
            secondSpan.CopyTo(spanResult);
            return result;
        }

        private static IntPtr GetIntPtr(int size) => Marshal.AllocHGlobal(size);
    }
}
