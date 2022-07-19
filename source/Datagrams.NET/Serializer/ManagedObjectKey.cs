
namespace DatagramsNet.Serializer
{
    internal readonly struct ManagedObjectKey
    {
        public Type ParentType { get; }

        public string Name { get; }

        public ManagedObjectKey(Type parentType, string name) 
        {
            ParentType = parentType;
            Name = name;
        }
    }
}
