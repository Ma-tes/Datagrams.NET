using DatagramsNet.Datagrams.NET.Logger;
using DatagramsNet.Datagrams.NET.Prefixes;
using System.Runtime.InteropServices;

namespace DatagramsNet
{
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
            MemoryHolder = new byte[Marshal.SizeOf(@object)];
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

            return (T)Marshal.PtrToStructure(objectPointer, typeof(T));
        }

        public void Dispose() 
        {
            Marshal.FreeHGlobal(@objectPointer);
        }

        private IntPtr GetIntPtr() => Marshal.AllocHGlobal(MemoryHolder.Length); //**(IntPtr**)(&reference);
    }
}
