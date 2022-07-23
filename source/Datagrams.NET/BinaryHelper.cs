using DatagramsNet.Serializer;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

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
            IntPtr objectPointer = GetIntPtr(bytes.Length);
            Marshal.Copy(bytes, 0, objectPointer, bytes.Length);

            if (typeof(T) == typeof(string)) 
            {
                return (T)(object)Encoding.ASCII.GetString(bytes);
            }

            var @object = Activator.CreateInstance<T>();
            var members = GetMembersInformation(@object).ToArray();
            if (members is not null) 
            {
                var @currentObject = (T)Serialization.DeserializeBytes(@object.GetType(), bytes);
                return @currentObject;
            }
            return (T)Marshal.PtrToStructure(objectPointer, typeof(T));
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
                return null;
            var members = new List<MemberInfo>();
            members.AddRange(objectType.GetFields());
            members.AddRange(objectType.GetProperties());
            return members;
        }

        //TODO: Create any type of caching
        public static int GetSizeOf<T>(T @object, ref byte[] bytes)
        {
            int size;
            if (@object is not null && @object.GetType().IsClass && @object is not string)
            {
                var membersInformation = GetMembersInformation(@object!).ToArray();
                var sizeTable = GetSizeOfClass(membersInformation, bytes);
                size = sizeTable;
            }
            else 
            {
                var holder = GetTableHolderInformation(new MemberInformation(@object, @object.GetType()), bytes, 0);
                size = holder.Length;
                bytes = holder.Bytes;
            }
            return size;
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

        private static MemberTableHolder GetTableHolderInformation(MemberInformation member, byte[] bytes, int start)
        {
            object memberObject = member.MemberValue;

            int size;
            if (memberObject is NullValue && bytes.Length > 1)
            {
                size = bytes[start];
                var bytesCopy = bytes.ToList();
                bytesCopy.RemoveAt(start);
                bytes = bytesCopy.ToArray();

                return new MemberTableHolder(bytes, size);
            }

            if (memberObject is string)
            {
                size = (((string)(memberObject)).Length * sizeof(byte));
                if (bytes.Length > 1) 
                {
                    var bytesCopy = bytes.ToList();
                    bytesCopy.RemoveAt(start);
                    bytes = bytesCopy.ToArray();

                    return new MemberTableHolder(bytes, size);
                }
                return new MemberTableHolder(bytes, size);
            }

            if (memberObject is Array)
            {
                var memberArray = ((Array)memberObject!);
                size = (Marshal.SizeOf(GetTypeOfArrayElement(memberArray)) * (memberArray.Length));

                return new MemberTableHolder(bytes, size);
            }

            size = Marshal.SizeOf(memberObject!);
            return new MemberTableHolder(bytes, size);
        }

        private static Type GetTypeOfArrayElement(Array objects) => objects.GetType().GetGenericArguments()[0];

        private static IntPtr GetIntPtr(int size) => Marshal.AllocHGlobal(size); //**(IntPtr**)(&reference);
    }
}
