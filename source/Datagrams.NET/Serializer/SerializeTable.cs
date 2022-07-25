
namespace DatagramsNet.Serializer
{
    internal readonly struct SerializeTable 
    {
        public object Object { get; }

        public byte[] Bytes { get; }

        public SerializeTable(object @object, byte[] bytes) 
        {
            Object = @object;
            Bytes = bytes;
        }
    }
}
