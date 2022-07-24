
namespace DatagramsNet.Serializer.Interfaces
{
    internal interface IManaged
    {
        public byte[] Serialize<TParent>(ObjectTableSize @object);

        public T Deserialize<T>(byte[] bytes);
    }
}
