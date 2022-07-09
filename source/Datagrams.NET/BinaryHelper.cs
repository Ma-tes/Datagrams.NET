using System.Runtime.InteropServices;

namespace DatagramsNet
{
    internal static class BinaryHelper
    {
        public static byte[] Write(object value)
        {
            var bytes = new byte[Marshal.SizeOf(value)];
            IntPtr ptr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.StructureToPtr(value, ptr, false);
            Marshal.Copy(ptr, bytes, 0, bytes.Length);
            return bytes;
        }

        public static T Read<T>(byte[] bytes)
        {
            IntPtr objectPointer = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, objectPointer, bytes.Length);
            if (typeof(T) == typeof(string))
                return (T)(object)Marshal.PtrToStringUTF8(objectPointer)!;
            return (T)Marshal.PtrToStructure(objectPointer, typeof(T))!;
        }
    }
}
