using DatagramsNet.Logging;
using DatagramsNet.Prefixes;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DatagramsNet
{
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct ObjectTableSize 
    {
        [FieldOffset(4)]
        public int Size;
    }
    public sealed class BinaryHelper : IDisposable //TODO: GC.AllocateArray();
    {
        public byte[] MemoryHolder { get; private set; }

        private IntPtr @objectPointer;

        private object binaryObject;

        public BinaryHelper(byte[] data) 
        {
            if (data is not null)
                MemoryHolder = data;
            else
                ServerLogger.Log<ErrorPrefix>($"{data.GetType()} in {nameof(BinaryHelper)} is null");
        }

        public BinaryHelper(object @object) 
        {
            binaryObject = @object;
            MemoryHolder = new byte[GetSizeOf(@object)];
            //MemoryHolder = new byte[Marshal.SizeOf(@object)];
            @objectPointer = GetIntPtr();
        }

        public bool Write() 
        {
            Marshal.StructureToPtr(binaryObject, @objectPointer, false);
            Marshal.Copy(@objectPointer, MemoryHolder, 0, MemoryHolder.Length);
            return MemoryHolder[0] != 0;
        }

        public T Read<T>()
        {
            IntPtr objectPointer = Marshal.AllocHGlobal(MemoryHolder.Length);
            Marshal.Copy(MemoryHolder, 0, objectPointer, MemoryHolder.Length);

            if (typeof(T) is string)
                return (T)(object)Marshal.PtrToStringUTF8(objectPointer);
            return (T)Marshal.PtrToStructure(objectPointer, typeof(T));
        }

        //TODO: Create any type of caching
        private int GetSizeOf<T>(T @object)
        {
            Type objectType = @object.GetType();
            int size;
            if (objectType.IsClass) 
            {
                var members = new List<MemberInfo>();
                members.AddRange(objectType.GetFields());
                members.AddRange(objectType.GetProperties());
                size = GetTotalSizeOf(members.ToArray(), @object);
            }
            else
                size = Marshal.SizeOf(@object);
            return size;
        }

        private int GetTotalSizeOf<T>(MemberInfo[] members, T @object)
        {
            int totalSize = 0;
            for (int i = 0; i < members.Length; i++)
            {
                var currentMember = members[i];
                //TODO: This must be done by generic method
                Type memberType = currentMember.MemberType is MemberTypes.Property ? ((PropertyInfo)currentMember).PropertyType : ((FieldInfo)currentMember).FieldType;
                object? memberObject = currentMember.MemberType is MemberTypes.Property ? ((PropertyInfo)currentMember).GetValue(@object) : ((FieldInfo)currentMember).GetValue(@object);
                if (memberType.IsArray) 
                {
                    var memberArray = ((Array)memberObject!);
                    totalSize = totalSize + (Marshal.SizeOf(GetTypeOfArrayElement(memberArray)) * (memberArray.Length));
                }
                else if(memberObject is string)
                    totalSize = totalSize + (((string)(memberObject)).Length * sizeof(byte));
                else
                    totalSize = totalSize + Marshal.SizeOf(memberObject!);
            }
            return totalSize + (totalSize / 2);
        }

        private Type GetTypeOfArrayElement(Array objects) => objects.GetType().GetGenericArguments()[0];

        public void Dispose() 
        {
            Marshal.FreeHGlobal(@objectPointer);
        }

        private IntPtr GetIntPtr() => Marshal.AllocHGlobal(MemoryHolder.Length); //**(IntPtr**)(&reference);
    }
}
