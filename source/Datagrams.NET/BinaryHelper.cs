using DatagramsNet.Serialization;
using DatagramsNet.Serialization.Interfaces;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DatagramsNet
{
    internal readonly struct SizedObject
    {
        public object? Value { get; }
        public int Size { get; }

        public SizedObject(object? value, int size)
        {
            Value = value;
            Size = size;
        }
    }

    public static class BinaryHelper
    {
        private static readonly MethodInfo deserialization = typeof(ManagedTypeFactory).GetMethod(nameof(ManagedTypeFactory.Deserialize))!;

        public static byte[] Write(object @object)
        {
            var byteHolder = ReadOnlyMemory<byte>.Empty;
            int size = GetSizeOf(@object, @object.GetType(), ref byteHolder);
            var buffer = Serializer.SerializeObject(@object, size);

            if (buffer is null)
            {
                var fixedBuffer = new byte[size];

                IntPtr pointer = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(@object, pointer, false);
                Marshal.Copy(pointer, fixedBuffer, 0, fixedBuffer.Length);
                return fixedBuffer;
            }

            return buffer;
        }

        public static unsafe T Read<T>(ReadOnlyMemory<byte> bytes)
        {
            if (Serializer.TryGetManagedType(typeof(T), out IManagedSerializer? managedType))
            {
                var currentObject = (T)(deserialization.MakeGenericMethod(typeof(T)).Invoke(null, new object[] { managedType, bytes }))!;
                return currentObject;
            }

            if (typeof(T).IsClass)
            {
                var @object = Activator.CreateInstance<T>();
                var members = GetMembersInformation(@object!).ToArray();
                if (members is not null)
                {
                    var @currentObject = (T)Serializer.DeserializeBytes(@object!.GetType(), bytes);
                    return @currentObject;
                }
            }

            IntPtr objectPointer = Marshal.AllocHGlobal(bytes.Length);
            bytes.Span.CopyTo(new Span<byte>(objectPointer.ToPointer(), bytes.Length));
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

        public static int GetSizeOf<T>(T @object, Type @objectType, ref ReadOnlyMemory<byte> bytes)
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
                var holder = GetTableHolderInformation(new MemberInformation(@object, objectType), bytes);
                size = holder.Length;
                bytes = holder;
            }
            return size;
        }

        private static ReadOnlyMemory<byte> GetTableHolderInformation(MemberInformation member, ReadOnlyMemory<byte> bytes)
        {
            object memberObject = member.MemberValue!;
            bool isManagedType = Serializer.TryGetManagedType(member.MemberType, out IManagedSerializer? _);
            int size;

            if ((isManagedType || memberObject is NullValue) && bytes.Length >= sizeof(int))
            {
                if (!isManagedType)
                {
                    size = Marshal.SizeOf(member.MemberType);
                    return bytes.Length >= size ? bytes[0..size] : bytes;
                }

                size = MemoryMarshal.Read<int>(bytes.Span);
                return bytes.Slice(sizeof(int), size);
            }


            if (memberObject is string text)
            {
                size = text.Length * sizeof(byte);
                return bytes.Length >= size ? bytes[0..size] : bytes;
            }

            if (memberObject is Array array)
            {
                size = GetSizeOfArray(array, ref bytes);
                return bytes.Length >= size ? bytes[0..size] : bytes;
            }

            size = Marshal.SizeOf(memberObject!);
            return bytes.Length >= size ? bytes[0..size] : bytes;
        }

        private static int GetSizeOfClass(MemberInformation[] members, ReadOnlyMemory<byte> bytes)
        {
            int totalSize = 0;
            int offset = 0;

            foreach (var member in members)
            {
                var tableHolder = GetTableHolderInformation(member, bytes[totalSize..]);
                int difference = bytes.Length - tableHolder.Length;

                bytes = tableHolder;
                totalSize += tableHolder.Length;
                offset += difference;
            }
            return totalSize + offset;
        }

        public static int GetSizeOfArray(Array array, ref ReadOnlyMemory<byte> bytes)
        {
            int totalSize = 0;
            for (int i = 0; i < array.Length; i++)
            {
                var elementValue = array.GetValue(i);
                int currentSize = GetSizeOf(elementValue, elementValue!.GetType(), ref bytes);
                totalSize += currentSize;
            }
            return totalSize;
        }
    }
}
