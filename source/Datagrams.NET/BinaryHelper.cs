using DatagramsNet.Logging;
using DatagramsNet.Prefixes;
using DatagramsNet.Serializer;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace DatagramsNet
{
    internal struct ObjectTableSize
    {
        public object Value { get; }

        public int Size { get; }

        public ObjectTableSize(object value, int size)
        {
            Value = value;
            Size = size;
        }
    }

    public static class BinaryHelper 
    {
        public static byte[] Write(object @object)
        {
            int size = GetSizeOf(@object);
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
        public static int GetSizeOf<T>(T @object)
        {
            int size;
            if (@object.GetType().IsClass && @object is not string)
            {
                var membersInformation = GetMembersInformation(@object!).ToArray();
                size = GetTotalSizeOf(membersInformation.ToArray());
            }
            else
                size = GetTotalSizeOf(new MemberInformation[] { new MemberInformation(@object, @object.GetType()) });
            return size;
        }

        private static int GetTotalSizeOf(MemberInformation[] membersInformation)
        {
            int totalSize = 0;
            for (int i = 0; i < membersInformation.Length; i++)
            {
                Type memberType = membersInformation[i].MemberType;
                object memberObject = membersInformation[i].MemberValue;
                int memberSize;
                if (memberObject is Array) 
                {
                    var memberArray = ((Array)memberObject!);
                    memberSize = (Marshal.SizeOf(GetTypeOfArrayElement(memberArray)) * (memberArray.Length));
                }
                else if(memberObject is string)
                    memberSize = (((string)(memberObject)).Length * sizeof(byte));
                else
                    memberSize = Marshal.SizeOf(memberObject!);
                totalSize = totalSize + memberSize;
            }
            return totalSize;//+ (totalSize / 2);
        }

        private static Type GetTypeOfArrayElement(Array objects) => objects.GetType().GetGenericArguments()[0];

        private static IntPtr GetIntPtr(int size) => Marshal.AllocHGlobal(size); //**(IntPtr**)(&reference);
    }
}
