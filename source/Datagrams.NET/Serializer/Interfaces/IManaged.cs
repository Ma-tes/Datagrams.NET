
namespace DatagramsNet.Serializer.Interfaces
{
    internal interface IManaged
    {
        public byte[] Serialize<TParent>(ObjectTableSize @object);
    }
}
